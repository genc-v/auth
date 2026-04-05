using System.IdentityModel.Tokens.Jwt;

using System.Security.Claims;
using System.Text;

using cmsUserManagment.Application.Common.Settings;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace cmsUserManagment.Middlewares;

public class JwtValidationMiddleware
{
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<JwtValidationMiddleware> _logger;
    private readonly RequestDelegate _next;

    public JwtValidationMiddleware(RequestDelegate next, ILogger<JwtValidationMiddleware> logger,
        IOptions<JwtSettings> jwtSettings)
    {
        _next = next;
        _logger = logger;
        _jwtSettings = jwtSettings.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        Endpoint? endpoint = context.GetEndpoint();
        if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
        {
            await _next(context);
            return;
        }

        string? authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Missing or invalid Authorization header.");
            return;
        }

        string token = authHeader.Substring("Bearer ".Length).Trim();

        try
        {
            JwtSecurityTokenHandler tokenHandler = new();
            byte[] key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);

            TokenValidationParameters validationParameters = new()
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            ClaimsPrincipal principal =
                tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

            context.User = principal;

            await _next(context);
        }
        catch (SecurityTokenExpiredException)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Token expired.");
        }
        catch (SecurityTokenException ex)
        {
            _logger.LogWarning(ex, "JWT validation failed.");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Invalid JWT token.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during JWT validation.");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsync("Internal server error.");
        }
    }
}