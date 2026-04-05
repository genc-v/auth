using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace cmsUserManagment.Infrastructure.Security;

public class JwtDecoder
{
    public Guid GetUserid(string jwtToken)
    {
        JwtSecurityTokenHandler handler = new();
        JwtSecurityToken? token = handler.ReadJwtToken(jwtToken);

        string? userId = token.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub")
            ?.Value;

        if (userId == null)
        {
            throw new ArgumentException("Invalid JWT token: userId claim not found.");
        }

        return Guid.Parse(userId);
    }
}
