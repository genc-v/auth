using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using cmsUserManagment.Application.Common.Settings;
using cmsUserManagment.Application.Interfaces;

using Microsoft.IdentityModel.Tokens;

namespace cmsUserManagment.Infrastructure.Security;

public class JwtTokenProvider : IJwtTokenProvider
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenProvider(JwtSettings jwtSettings)
    {
        _jwtSettings = jwtSettings;
    }

    public string GenerateToken(string email, string id, bool isAdmin)
    {
        SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(_jwtSettings.Secret));
        SigningCredentials credentials = new(securityKey, SecurityAlgorithms.HmacSha256);

        List<Claim> claims = new()
        {
            new Claim(JwtRegisteredClaimNames.Sub, id), new Claim(JwtRegisteredClaimNames.Email, email)
        };

        string role = "user";

        if (isAdmin) role = "admin";

        claims.Add(new Claim(ClaimTypes.Role, role));

        SecurityTokenDescriptor tokenDescriptor = new()
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
            SigningCredentials = credentials,
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience
        };

        JwtSecurityTokenHandler tokenHandler = new();
        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
