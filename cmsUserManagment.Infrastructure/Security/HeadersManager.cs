using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace cmsUserManagment.Infrastructure.Security;

public class HeadersManager
{
    public string GetJwtFromHeader(IHeaderDictionary headers)
    {
        if (headers.TryGetValue("Authorization", out StringValues authHeader)
            && !StringValues.IsNullOrEmpty(authHeader)
            && authHeader.ToString().StartsWith("Bearer "))
            return authHeader.ToString().Substring("Bearer ".Length);

        throw new UnauthorizedAccessException("Missing or invalid Authorization header.");
    }
}
