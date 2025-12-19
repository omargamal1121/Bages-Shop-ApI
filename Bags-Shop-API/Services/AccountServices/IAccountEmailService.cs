namespace Bags_Shop_API.Services.AccountServices
{
    public interface IAccountEmailService
    {
        Task SendValidationEmailAsync(string email, string userId, string token, string frontendUrl);
        Task SendEmailAfterChangePassAsync(string username, string email);
        Task SendPasswordResetEmailAsync(string email, string username, string token);
        Task SendPasswordResetSuccessEmailAsync(string email);
        Task SendAccountLockedEmailAsync(string email, string username, string reason = "Multiple failed login attempts");
        Task SendWelcomeEmailAsync(string email, string username);
    }
}
