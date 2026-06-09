using System.Net.Http.Json;

using cmsUserManagment.Application.Interfaces;

using Microsoft.Extensions.Logging;

namespace cmsUserManagment.Infrastructure.Services;

public class ExpoPushService : IExpoPushService
{
    private const string ExpoPushUrl = "https://exp.host/--/api/v2/push/send";

    private readonly HttpClient _httpClient;
    private readonly ILogger<ExpoPushService> _logger;

    public ExpoPushService(HttpClient httpClient, ILogger<ExpoPushService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task SendAsync(IEnumerable<string> expoTokens, string title, string body, object? data = null)
    {
        var messages = expoTokens
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => new
            {
                to = t,
                title,
                body,
                sound = "default",
                data
            })
            .ToList();

        if (messages.Count == 0) return;

        try
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(ExpoPushUrl, messages);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send Expo push notification to {Count} device(s)", messages.Count);
        }
    }
}
