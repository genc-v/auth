using System.Net;
using System.Text.Json;

using cmsUserManagment.Application.Common.ErrorCodes;

namespace cmsUserManagment.Middlewares;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;

    public ErrorHandlingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AuthErrorCodes ex)
        {
            await HandleCustomErrorAsync(context, ex.Code, ex.Message, HttpStatusCode.Unauthorized);
        }
        catch (GeneralErrorCodes ex)
        {
            var status = ex.Code switch
            {
                1 => HttpStatusCode.NotFound,
                6 => HttpStatusCode.Conflict,
                8 => HttpStatusCode.Forbidden,
                9 => HttpStatusCode.BadRequest,
                _ => HttpStatusCode.BadRequest
            };
            await HandleCustomErrorAsync(context, ex.Code, ex.Message, status);
        }
        catch (Exception ex)
        {
            await HandleGenericErrorAsync(context, ex);
        }
    }

    private static Task HandleCustomErrorAsync(
        HttpContext context,
        int code,
        string message,
        HttpStatusCode status)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int) status;

        string result = JsonSerializer.Serialize(new { Code = code, Message = message });

        return context.Response.WriteAsync(result);
    }

    private static Task HandleGenericErrorAsync(HttpContext context, Exception error)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;

        string result = JsonSerializer.Serialize(new { Code = -1, error.Message, Inner = error.InnerException?.Message, Inner2 = error.InnerException?.InnerException?.Message });

        return context.Response.WriteAsync(result);
    }
}
