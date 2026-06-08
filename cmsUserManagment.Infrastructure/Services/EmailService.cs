using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using cmsUserManagment.Application.Common.ErrorCodes;
using cmsUserManagment.Application.Common.Settings;
using cmsUserManagment.Application.Interfaces;

namespace cmsUserManagment.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly HttpClient _httpClient;
    private readonly EmailSettings _settings;

    public EmailService(HttpClient httpClient, IOptions<EmailSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetCode)
    {
        try
        {
            var payload = new { toEmail, resetCode };
            var response = await _httpClient.PostAsJsonAsync(_settings.ResetCodeApiUrl, payload);
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            throw new GeneralErrorCodes(10, $"Email service unreachable — {ex.Message}");
        }
    }
}
