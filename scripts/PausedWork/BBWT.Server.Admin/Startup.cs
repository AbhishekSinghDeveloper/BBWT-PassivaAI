using AspNetCoreRateLimit;

using Autofac;

using AutoMapper;

using BBWM.Core.Audit;
using BBWM.Core.Data;
using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Model;
using BBWM.Core.ModelHashing;
using BBWM.Core.ModuleLinker;
using BBWM.Core.Web.Filters;
using BBWM.Core.Web.Middlewares;
using BBWM.Core.Web.ModelBinders;
using BBWM.Metadata;
using BBWM.SystemSettings;

using BBWT.Data;
using BBWT.Data.Model;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;

using System.Globalization;

namespace BBWT.Server.Admin;

public class Startup
{
    private IWebHostEnvironment Environment { get; }

    private IConfiguration Configuration { get; }

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
            AddBufferLog("ConfigureServices()");
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // configure Identity options
            Action<IdentityOptions> identityOptions = options =>
            {
                options.SignIn.RequireConfirmedEmail = false;

                // Lockout settings by default
                options.Lockout.AllowedForNewUsers = false;
                options.Lockout.MaxFailedAccessAttempts = 100;

                // options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);

                // Password settings
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 0;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;

                // User settings
                // options.User.RequireUniqueEmail = true;
            };

            // configure refresh impersonation data (https://tech.trailmax.info/2017/07/user-impersonation-in-asp-net-core/)
            services.Configure<SecurityStampValidatorOptions>(options => // different class name
            {
                options.ValidationInterval = TimeSpan.FromMinutes(10); // new property name
                options.OnRefreshingPrincipal = context => // new property name
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

            // Gets db connection settings
            var databaseConnectionSettings = Configuration.GetSection("DatabaseConnectionSettings").Get<DatabaseConnectionSettings>();
            AddBufferLog($"Database type: {databaseConnectionSettings.DatabaseType}");

            string connectionString;
            string auditConnection;

            if (databaseConnectionSettings.DatabaseType == DatabaseType.MySql)
            {
                connectionString = Configuration.GetConnectionString("MySqlConnection");
                auditConnection = Configuration.GetConnectionString("AuditMySqlConnection");

                services.AddAuditMySQLDataContext(databaseConnectionSettings, auditConnection);
                services.AddBBWTMySQLDataContext(databaseConnectionSettings, connectionString, identityOptions);
            }
            else if (databaseConnectionSettings.DatabaseType == DatabaseType.MsSql)
            {
                connectionString = Configuration.GetConnectionString("DefaultConnection");
                auditConnection = Configuration.GetConnectionString("AuditConnection");

                services.AddAuditSqlServerDataContext(databaseConnectionSettings, auditConnection);
                services.AddBBWTSqlServerDataContext(databaseConnectionSettings, connectionString, identityOptions);
            }

            services.AddCors();

            services.AddSpecificServices();
            services.AddFilters();

            services.AddOptions();

            services.AddAutoMapper(cfg =>
                cfg.AddMetadataMapping<Metadata>(), ModuleLinker.GetBbAssemblies());

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

            // https://github.com/aspnet/Hosting/issues/793
            // the IHttpContextAccessor service is not registered by default.
            // the clientId/clientIp resolvers use it.
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // configure the resolvers
            services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

            // inject counter and rules stores
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IClientPolicyStore, MemoryCacheClientPolicyStore>();
            var modelHashingService = new ModelHashingService();
            services.AddSingleton<IModelHashingService>(modelHashingService);
            modelHashingService.IgnoreModelHashing<LoginAuditDTO>();
            modelHashingService.IgnorePropertiesHashing<LoginAuditDTO>(a => a.Id);

            // Set the default authentication policy to require users to be authenticated.
            // You can opt out of authentication at the controller or action method with the [AllowAnonymous] attribute.
            // With this approach, any new controllers added will automatically require authentication,
            // which is safer than relying on new controllers to include the [Authorize] attribute.
            // services.AddRouting(options => options.LowercaseUrls = true);
            AddBufferLog("Adding services middleware");
            services.AddResponseCaching();
            services.AddControllersWithViews(
                config =>
                {
                    var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();
                    config.Filters.Add(new AuthorizeFilter(policy));
                    config.Filters.Add(new ApiVersionAttribute(Environment));
                    config.Filters.Add(new ResponseCacheAttribute
                    { NoStore = true, Location = ResponseCacheLocation.None });

                    config.ModelBinderProviders.Insert(0, new FilterInfoModelBinderProvider());
                });

            services.AddControllers().AddNewtonsoftJson(options =>
            {
                // Don't remove the error detections! Removing of error detections leads to stack overflow and server crash.
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Error;
                options.SerializerSettings.Converters.Add(new GlobalHashKeyJsonConverter(JsonSerializer.CreateDefault(options.SerializerSettings), modelHashingService));

                options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
            });

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

            #region Link modules services
            var linkers = ModuleLinker.GetInstances<IServicesModuleLinkage>();
            linkers.ForEach(o =>
            {
                try { o.ConfigureServices(services, Configuration); }
                catch (Exception ex)
                {
                    ModuleLinker.InvokeExceptions.Add(ex);
                }
            });
            #endregion
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(
        IApplicationBuilder app,
        ISettingsSectionService sectionsService,
        IOptions<MvcNewtonsoftJsonOptions> newtonsoftJsonOptions,
        ILogger<Startup> logger)
    {
        FlushLogsBuffer(logger);
        app.RegisterBbwtSettings();

        sectionsService.SetJsonSerializerSettings(newtonsoftJsonOptions.Value.SerializerSettings);

        AddBufferLog("Before app.UseSecurityHeaders()");
        var policyCollection = new HeaderPolicyCollection()
            .AddFrameOptionsSameOrigin()
            .AddContentTypeOptionsNoSniff()
            .AddXssProtectionBlock()
            .RemoveCustomHeader("X-Powered-By");
        app.UseSecurityHeaders(policyCollection);

        AddBufferLog("Before app.UseIpRateLimiting()");
        app.UseIpRateLimiting();

        var isDevelopment = Environment.IsDevelopment();
        using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            if (isDevelopment)
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseErrorHandlingMiddleware();

            // set db descriptions
            var context = serviceScope.ServiceProvider.GetService<IDataContext>();
            var mapper = serviceScope.ServiceProvider.GetService<IMapper>();
            var modelHashingService = serviceScope.ServiceProvider.GetService<IModelHashingService>();
            modelHashingService.Register(mapper, context);
        }

        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
        });

        AddBufferLog("Configuring locale");
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

        if (Convert.ToBoolean(Configuration["ENABLE_HTTPS_REDIRECT"]))
        {
            AddBufferLog("Adding UseHttpToHttpsRedirectMiddleware");
            app.UseHttpToHttpsRedirectMiddleware();
        }

        app.UseRouting();

        AddBufferLog("Adding middleware");
        app.UseResponseCompression();

        app.UseCors(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().AllowCredentials());

        AddBufferLog("Before app.UseAuthentication()");
        app.UseAuthentication();

        app.UseAuthorization();

        AddBufferLog("Adding UseAddUserIdToLogsMiddleware()");
        app.UseAddUserToLogsMiddleware();

        AddBufferLog("Before app.UseEndpoints()");
        app.UseEndpoints(builder =>
        {
            builder.MapControllerRoute("default", "{controller}/{action=Index}/{id?}");
        });
        app.UseResponseCaching();
    }

    public void ConfigureContainer(ContainerBuilder builder)
    {
        builder.AddBBWTServices();

        var linkers = ModuleLinker.GetInstances<IDependenciesModuleLinkage>();
        linkers.ForEach(o =>
        {
            try { o.RegisterDependencies(builder); }
            catch (Exception ex)
            {
                ModuleLinker.InvokeExceptions.Add(ex);
            }
        });
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

}
