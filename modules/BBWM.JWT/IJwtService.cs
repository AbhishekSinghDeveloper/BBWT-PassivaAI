namespace BBWM.JWT;

public interface IJwtService
{
    JwtInfo GenerateToken(string username);
    JwtInfo GenerateReportToken(string username, string report);
}
