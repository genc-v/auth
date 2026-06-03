using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

using Microsoft.AspNetCore.SignalR;

namespace cmsUserManagment.Infrastructure.SignalR;

public class UserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection)
    {
        return connection.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
               ?? connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}
