using AspNetCoreRateLimit;

using Autofac;

using BBWM.AWS.EventBridge.Api;
using BBWM.Core;
using BBWM.Core.Autofac;
using BBWM.Core.Data;
using BBWM.Core.Membership;
using BBWM.Core.Membership.Filters;
using BBWM.Core.Membership.Model;
using BBWM.Core.ModelHashing;
using BBWM.Core.ModuleLinker;
using BBWM.Core.Security;
using BBWM.Core.Utils;
using BBWM.Core.Web.CookieAuth;
using BBWM.Core.Web.Filters;
using BBWM.Core.Web.JsonConverters;
using BBWM.Core.Web.Middlewares;
using BBWM.Core.Web.ModelBinders;
using BBWM.Core.Web.OData;
using BBWM.SystemSettings;

using BBWT.Server.Extensions;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HostFiltering;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;

using Mindscape.Raygun4Net.AspNetCore;

using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BBWT.Server;

public partial class Startup
{
    private const string ApiPath = "/api";

    private IWebHostEnvironment Environment { get; }
    private IConfiguration Configuration { get; }
    public IContainer Container { get; private set; }
    public IApplicationBuilder _app { get; private set; }

    // Logs buffer is used to collect logs before the Configure() method. Core 3 allows the logger to be used by Configure() only.
    private readonly List<(DateTime, string, Exception)> LogsBuffer = new();

    public Startup(IConfiguration configuration, IWebHostEnvironment env)
    {
        Configuration = configuration;
        Environment = env;

        // for the ExcelDataReader library working
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
    }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        try
        {
            services.AddHttpClient();
            AddBufferLog("Started ConfigureServices()");
            AddBufferLog($"Configuration's database type: {Configuration.GetDatabaseConnectionSettings().DatabaseType}");

            services.AddSignalR();

            var cookieAuthSettings = Configuration.GetSection(CookieAuthSettings.SectionName).Get<CookieAuthSettings>();
            // configure refresh impersonation data (https://tech.trailmax.info/2017/07/user-impersonation-in-asp-net-core/)
            services.Configure<AuthSecurityStampValidatorOptions>(options =>
            {
                options.ValidationInterval = TimeSpan.FromSeconds(cookieAuthSettings.SecurityStampValidationInterval);
                options.AuthValidationInterval = TimeSpan.FromSeconds(cookieAuthSettings.AuthSecurityStampValidationInterval);
                options.OnRefreshingPrincipal = context =>
                {
                    var originalUserIdClaim = context.CurrentPrincipal.FindFirst(ClaimTypes.Impersonation.OriginalUserId);
                    var isImpersonatingClaim = context.CurrentPrincipal.FindFirst(ClaimTypes.Impersonation.IsImpersonating);
                    var originalUserNameClaim = context.CurrentPrincipal.FindFirst(ClaimTypes.Impersonation.OriginalUserName);

                    if (isImpersonatingClaim is null || isImpersonatingClaim.Value != bool.TrueString || originalUserIdClaim is null) return Task.FromResult(0);

                    context.NewPrincipal.Identities.First().AddClaim(originalUserIdClaim);
                    context.NewPrincipal.Identities.First().AddClaim(originalUserNameClaim);
                    context.NewPrincipal.Identities.First().AddClaim(isImpersonatingClaim);
                    return Task.FromResult(0);
                };
            });

            if (Environment.IsDevelopment())
            {
                services.AddCors();
            }

            services.AddSpecificServices();
            services.AddFilters();
            services.AddOptions();
            services.ConfigureFileStorage(Configuration, Environment);

            // Angular's default header name for sending the XSRF token.
            services.AddAntiforgery(options =>
            {
                if (!Environment.IsDevelopment())
                {
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                }

                options.HeaderName = "X-XSRF-TOKEN";
            });

            // AspNetCoreRateLimit
            // needed to store rate limit counters and ip rules
            services.AddMemoryCache();

            // load general configuration from appsettings.json
            services.Configure<IpRateLimitOptions>(Configuration.GetSection("IpRateLimiting"));

            // load ip rules from appsettings.json
            services.Configure<IpRateLimitPolicies>(Configuration.GetSection("IpRateLimitPolicies"));

            // load general configuration from appsettings.json
            services.Configure<ClientRateLimitOptions>(Configuration.GetSection("ClientRateLimiting"));

            // load client rules from appsettings.json
            services.Configure<ClientRateLimitPolicies>(Configuration.GetSection("ClientRateLimitPolicies"));

            // configure the resolvers
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();

            // inject counter and rules stores
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IClientPolicyStore, MemoryCacheClientPolicyStore>();
            var modelHashingService = new ModelHashingService();
            services.AddSingleton<IModelHashingService>(modelHashingService);

            // Set the default authentication policy to require users to be authenticated.
            // You can opt out of authentication at the controller or action method with the [AllowAnonymous] attribute.
            // With this approach, any new controllers added will automatically require authentication,
            // which is safer than relying on new controllers to include the [Authorize] attribute.
            // services.AddRouting(options => options.LowercaseUrls = true);
            services.AddResponseCaching();

            services.AddControllersWithViews(mvcOptions =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();

                var restrictingTypes = new[] {
                        typeof(ReadWriteAuthorizeAttribute),
                        typeof(AuthorizeEventBridgeAttribute)
                };

                mvcOptions.Filters.Add(new GlobalRestrictedAuthorizeFilter(policy, restrictingTypes));
                mvcOptions.Filters.Add(typeof(User2FaAccessFilter));
                mvcOptions.Filters.Add(new ApiVersionAttribute(Environment));
                mvcOptions.Filters.Add(new ResponseCacheAttribute { NoStore = true, Location = ResponseCacheLocation.None });

                // Replaces "<key>_original" propertyName-s to "<key>"
                mvcOptions.AddOriginalFiltersFixingValueProvider();

                #region Model binder providers
                var modelBinderProviders = new List<IModelBinderProvider>()
                {
                    #region Core providers
                    new FilterInfoModelBinderProvider(),
                    new HashedKeyBinderProvider(),
                    new FormDataJsonBinderProvider()
                    #endregion
                };

                // Module linker: get modules model binder providers
                ModuleLinker.RunLinkers<IConfigureMvcModuleLinkage>(
                    linker => modelBinderProviders.AddRange(linker.GetModelBinderProviders()));

                modelBinderProviders.ForEach(provider => mvcOptions.ModelBinderProviders.Insert(0, provider));
                #endregion

                mvcOptions.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
            });

            services
                .AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                    JsonSerializerOptionsProvider.OptionsWithoutCustomConverters =
                        new JsonSerializerOptions(options.JsonSerializerOptions);

                    options.JsonSerializerOptions.Converters.Add(new GlobalHashKeyJsonConverterFactory(modelHashingService));
                    options.JsonSerializerOptions.Converters.Add(new ObjectConverter());
                    JsonSerializerOptionsProvider.Options = new JsonSerializerOptions(options.JsonSerializerOptions);
                })
                .AddOData(SetupOData);

            services.AddSpaStaticFiles(options =>
            {
                options.RootPath = "wwwroot";
            });

            var authBuilder = services.AddAuthentication();

            if (!Environment.IsDevelopment())
            {
                services.AddRaygun(Configuration);
            }

            // Response compression
            services.Configure<GzipCompressionProviderOptions>(options => options.Level = System.IO.Compression.CompressionLevel.Optimal);
            services.AddResponseCompression(options =>
            {
                options.MimeTypes = new[]
                {
                    // Default
                    "text/plain",
                    "text/css",
                    "application/javascript",
                    "text/html",
                    "application/xml",
                    "text/xml",
                    "application/json",
                    "text/json",
                    // Custom
                    "image/svg+xml"
                };
            });

            services.ConfigureSecurity();

            #region Automapper

            var bbAssemblies = ModuleLinker.GetBbAssemblies();
            services.AddAutoMapper(ProfileBase.CollectAndRegisterMappings);
            services.AddAutoMapper(bbAssemblies);

            // Automatically adds default mapping of entities to DTO and reverse (by naming template [entity_name] <-> [entity_name]DTO).
            services.AddAutoMapper(cfg => ProfileBase.AutomapEntities(cfg, bbAssemblies));

            #endregion Automapper

            // Add project data contexts
            services.AddProjectDataContexts(Configuration);

            // DB contexts of modules
            services.AddModulesDataContexts(Configuration);

            #region host filtering issue 294-954
            // https://pts.bbconsult.co.uk/issueEditor?type=2&id=294954
            if (!Environment.IsDevelopment())
            {
                List<string> hostList = Configuration["AllowedHosts"]?
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new() {"N/A" };

                if (hostList.Any())
                {
                    AddBufferLog($"Allowed Hosts: {Configuration["AllowedHosts"]}");
                    if (hostList.All(h => h.Equals("N/A", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        AddBufferLog("The environmental variable 'AllowedHosts' is not set. " +
                            "The site cannot function without at least one hostname set. Hostnames must be specified to mitigate the security vulnerability known as 'Host Header Injection'. " +
                            "Further details of the vulnerability can be found online at OWASP.");
                    }
                        services.Configure<HostFilteringOptions>(options => options.AllowedHosts = hostList);
                }
            }
            #endregion

            Func<IServiceProvider> getAppBuilder = () => _app.ApplicationServices;

            // Module Linker: configure services
            ModuleLinker.RunLinkers<IServicesModuleLinkage>(linker =>
                linker.ConfigureServices(services, Configuration));

            // Module Linker: modules implementing authentication 
            ModuleLinker.RunLinkers<IAuthenticationModuleLinkage>(linker =>
                linker.Register(authBuilder, services, Configuration, getAppBuilder));

            ModuleLinker.InvokeExceptions.ForEach(ex => AddBufferLog($"Module linkage exception", ex));

            // For more info go to task:
            //  - https://pts.bbconsult.co.uk/taskEditor?id=206063
            services.AddHsts(opts =>
            {
                opts.IncludeSubDomains = true;
                opts.MaxAge = TimeSpan.FromDays(365);
                opts.Preload = true;
                opts.ExcludedHosts.Clear();
            });

            services.AddHttpsRedirection(options =>
            {
                options.RedirectStatusCode = (int)HttpStatusCode.PermanentRedirect;
                options.HttpsPort = 443;
            });

            AddBufferLog("Finished ConfigureServices()");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            AddBufferLog($"ConfigureServices() general exception", e);
        }
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(
        IApplicationBuilder app,
        IHostApplicationLifetime applicationLifetime,
        ISettingsSectionService sectionsService,
        IOptions<JsonOptions> jsonOptions,
        ILogger<Startup> logger)
    {
        FlushLogsBuffer(logger);

        app.RegisterBbwtSettings();

        //Module linker: Configure modules
        ModuleLinker.RunLinkers<IConfigureModuleLinkage>(linker => linker.ConfigureModule(app));

        sectionsService.SetJsonSerializerSettings(jsonOptions.Value.JsonSerializerOptions);

        logger.LogDebug("Before app.UseSecurityHeaders()");

        var cspOptions = app.ApplicationServices.GetService<IOptions<ContentSecurityPolicyOptions>>()?.Value ?? new();

        app.UseSecurityHeaders(
            new HeaderPolicyCollection()
                .AddFrameOptionsSameOrigin()
                .AddContentTypeOptionsNoSniff()
                .AddXssProtectionBlock()
                .RemoveCustomHeader("X-Powered-By")
                .RemoveServerHeader()
                .AddContentSecurityPolicy(cspOptions)
                .AddReferrerPolicyNoReferrer()
                .AddPermissionsPolicy(builder =>
                {
                    builder.AddAccelerometer().None();
                    builder.AddAmbientLightSensor().None();
                    builder.AddAutoplay().None();
                    builder.AddCamera().None();
                    builder.AddGeolocation().None();
                    builder.AddGyroscope().None();
                    builder.AddMagnetometer().None();
                    builder.AddMicrophone().None();
                    builder.AddMidi().None();
                    builder.AddPayment().None();
                    builder.AddPictureInPicture().None();
                    builder.AddSpeaker().None();
                    builder.AddUsb().None();
                    builder.AddCustomFeature("battery", "()");
                    builder.AddCustomFeature("display-capture", "()");
                    builder.AddCustomFeature("execution-while-not-rendered", "()");
                    builder.AddCustomFeature("execution-while-out-of-viewport", "()");
                    builder.AddCustomFeature("gamepad", "()");
                    builder.AddCustomFeature("screen-wake-lock", "()");
                    builder.AddCustomFeature("serial", "()");
                    builder.AddCustomFeature("xr-spatial-tracking", "()");
                }));
        app.UseSecurityHeadersMiddleware();

        logger.LogDebug("Before app.UseIpRateLimiting()");
        app.UseIpRateLimiting();

        app.UseErrorHandlingMiddleware();

        InitDatabasesAndData(app, applicationLifetime, logger);

        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
        });


        if (!Environment.IsDevelopment())
        {
            app.UseRaygun();
            app.UseHsts();
            app.UseHttpsRedirection();
        }

        logger.LogDebug("Configuring locale");
        var locale = Configuration["SiteLocale"];
        if (!string.IsNullOrWhiteSpace(locale))
        {
            var localizationOptions = new RequestLocalizationOptions
            {
                SupportedCultures = new List<CultureInfo> { new CultureInfo(locale) },
                SupportedUICultures = new List<CultureInfo> { new CultureInfo(locale) },
                DefaultRequestCulture = new RequestCulture(locale)
            };
            app.UseRequestLocalization(localizationOptions);
        }

            // 299-184 this feature should always on
        logger.LogDebug("Adding UseHttpToHttpsRedirectMiddleware");
        app.UseHttpToHttpsRedirectMiddleware();

        app.UseRouting();

        logger.LogDebug("Adding middleware");
        app.UseResponseCompression();

        var spaServerUrl = Configuration.GetValue<string>("SpaDevelopmentServer");

        if (Environment.IsDevelopment())
        {
            app.UseCors(p =>
            {
                p.AllowAnyHeader();
                p.AllowAnyMethod();
                p.AllowCredentials();
                p.WithOrigins(spaServerUrl);
            });
        }

        logger.LogDebug("Before app.UseAuthentication()");
        app.UseAuthentication();

        app.UseAuthorization();

        app.UseDisableSlidingExpirationMiddleware();

        logger.LogDebug("Adding UseAddUserIdToLogsMiddleware()");
        app.UseAddUserToLogsMiddleware();

        if (cspOptions.Enabled)
            app.UseSetContentSecurityPolicyNonce();

        app.UseStaticFiles();
        app.UseSpaStaticFiles();


        // Allow the serving of locally uploaded files
        if (Environment.IsDevelopment())
        {
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Environment.ContentRootPath, "data", "images")),
                RequestPath = "/data/images"
            });
        }

        app.UseAntiforgeryToken(new[] { ApiPath });

        logger.LogDebug("Before app.UseEndpoints()");
        app.UseEndpoints(endpoints =>
        {
            // Module linker: SignalR hubs
            ModuleLinker.RunLinkers<ISignalRModuleLinkage>(linker => linker.MapHubs(endpoints));

            endpoints.MapControllerRoute("default", "{controller}/{action=Index}/{id?}");
        });

        app.UseSpa(spa =>
        {
            spa.Options.SourcePath = Environment.IsDevelopment() ? "./" : "wwwroot";

            if (!string.IsNullOrEmpty(spaServerUrl))
            {
                spa.UseProxyToSpaDevelopmentServer(spaServerUrl);
            }
        });

        app.UseResponseCaching();
        app.UseHostFiltering();

        _app = app;

        applicationLifetime.ApplicationStopped.Register(() =>
        {
            Container?.Dispose();
        });
    }

    private void InitDatabasesAndData(
        IApplicationBuilder app,
        IHostApplicationLifetime applicationLifetime,
        ILogger<Startup> logger)
    {
        // "migrate" parameter should be passed from the migration CI job to run the app only to apply migrations and exit.
        // (This option is defined as app argument)
        var isMigrationsAppRun = Configuration.GetValue<bool>("migrate");

        app.InitDatabases(Configuration, applicationLifetime, Environment, isMigrationsAppRun, logger);

        if (isMigrationsAppRun)
        {
            applicationLifetime.StopApplication();
        }
        // Initial data is not seeded in the migration mode. It's seeded only on app startup.
        else
        {
            app.EnsureApplicationInitialData(Configuration, applicationLifetime, Environment, logger);
        }
    }

    public void ConfigureContainer(ContainerBuilder builder)
    {
        try
        {
            AddBufferLog("Started ConfigureContainer()");

            builder.RegisterBbwtServices();

            //Module linker: services dependencies
            ModuleLinker.RunLinkers<IDependenciesModuleLinkage>(linker => linker.RegisterDependencies(builder));

            // Automatically registers services following the convention.
            // See  <see cref="NonRegisteredServicesRegistrationSource"/> for more help.
            builder.RegisterSource(new NonRegisteredServicesRegistrationSource(ModuleLinker.GetBbAssemblies()));
            AddBufferLog("Finished ConfigureContainer()");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            AddBufferLog($"ConfigureContainer() general exception", e);
        }
    }

    private void AddBufferLog(string message, Exception ex = null)
        => LogsBuffer.Add((DateTime.Now, message, ex));

    private void FlushLogsBuffer(ILogger<Startup> logger)
    {
        LogsBuffer.ForEach(o =>
        {
            if (o.Item3 is null)
                logger.LogDebug($"[{o.Item1}] {o.Item2}");
            else
                logger.LogDebug(o.Item3, $"[{o.Item1}] {o.Item2}");
        });
        LogsBuffer.Clear();
    }

    private void SetupOData(ODataOptions options, IServiceProvider provider)
        => options
            .Select().Filter()
            .OrderBy().Expand()
            .Count().SkipToken()
            .SetMaxTop(50).AddRouteComponents("odata", GetEdmModel(provider));

    private static IEdmModel GetEdmModel(IServiceProvider provider)
    {
        var builder = new ODataConventionModelBuilder();
        builder.EnableLowerCamelCase();

        //Module linker: ODATA
        ModuleLinker.RunLinkers<IODataEntitySetsModuleLinkage>(linker => linker.AddEntitySets(builder, provider));

        return builder.GetEdmModel();
    }
}