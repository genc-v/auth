namespace cmsUserManagment.Application.Interfaces;

public interface IExpoPushService
{
    Task SendAsync(IEnumerable<string> expoTokens, string title, string body, object? data = null);
}
