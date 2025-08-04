using BBWM.Core.Exceptions;
using BBWM.Core.ModuleLinker;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

using System.Text;

namespace BBWM.JWT;

public class ServicesModuleLinkage : IServicesModuleLinkage, IAuthenticationModuleLinkage
{
    private const string JwtSection = "Jwt";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(JwtSection);
        var settings = section.Get<JwtSettings>();
        if (settings is null)
            throw new EmptyConfigurationSectionException(JwtSection);
        if (string.IsNullOrWhiteSpace(settings.Key))
            throw new EmptyConfigurationSectionException($"{JwtSection}.{nameof(JwtSettings.Key)}");
        services.Configure<JwtSettings>(section);
    }

    public void Register(AuthenticationBuilder authBuilder, IServiceCollection services, IConfiguration configuration, Func<IServiceProvider> getServicesProvider)
    {
        var settings = configuration.GetSection(JwtSection)?.Get<JwtSettings>();

        if (string.IsNullOrWhiteSpace(settings?.Key)) return;

        services.Configure((AuthenticationOptions options) =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        });

        authBuilder.AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(settings.Key)),
                ValidateIssuerSigningKey = true,
                ValidIssuer = settings.Issuer,
                ValidateIssuer = !string.IsNullOrWhiteSpace(settings.Issuer),
                ValidAudience = settings.Audience,
                ValidateAudience = !string.IsNullOrWhiteSpace(settings.Audience)
            };
        });
    }
}
