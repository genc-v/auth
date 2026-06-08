namespace cmsUserManagment.Application.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string resetCode);
}
