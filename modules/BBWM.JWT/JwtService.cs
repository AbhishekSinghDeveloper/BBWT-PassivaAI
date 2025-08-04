using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BBWM.JWT;

public class JwtService : IJwtService
{
    private readonly JwtSettings _jwtSettings;

    public JwtService(IOptionsSnapshot<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public JwtInfo GenerateToken(string username)
    {
        var claims = new Claim[] { new Claim(ClaimTypes.Name, username) };
        return this.Generate(claims);
    }

    public JwtInfo GenerateReportToken(string username, string reportPath)
    {
        var claims = new Claim[] { new Claim(ClaimTypes.Name, username), new Claim("ReportPath", reportPath) };
        return this.Generate(claims);
    }

    private JwtInfo Generate(Claim[] claims)
    {
        var now = DateTime.UtcNow;

        if (_jwtSettings is not null && !string.IsNullOrEmpty(_jwtSettings.Key) && !string.IsNullOrEmpty(_jwtSettings.Audience))
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings?.Key);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return new JwtInfo
            {
                Token = tokenHandler.WriteToken(token),
                Audience = _jwtSettings?.Audience
            };
        }

        return null;
    }
}
