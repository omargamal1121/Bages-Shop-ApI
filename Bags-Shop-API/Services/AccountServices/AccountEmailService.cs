using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using System.Net;
using System.Text;

namespace Bags_Shop_API.Services.AccountServices
{
    public class AccountEmailService : IAccountEmailService
    {
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountEmailService> _logger;

        public AccountEmailService(
            IConfiguration configuration,
            IEmailSender emailSender,
            ILogger<AccountEmailService> logger)
        {
            _configuration = configuration;
            _emailSender = emailSender;
            _logger = logger;
        }

        public async Task SendValidationEmailAsync(string email, string userId, string token, string frontendUrl)
        {
            try
            {
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                var confirmationLink = $"{frontendUrl}/api/Account/confirm-email?userId={userId}&token={encodedToken}";

                string subject = "Email Confirmation - Welcome to Our Service";
                string message = CreateEmailTemplate(
                    "Email Confirmation",
                    $@"
                    <h1 style='color: #2c3e50; margin-bottom: 20px;'>Welcome to Our Service!</h1>
                    <p style='font-size: 16px; line-height: 1.6; color: #34495e;'>
                        Thank you for registering with us. To complete your registration, please confirm your email address by clicking the button below.
                    </p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{confirmationLink}' style='background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 12px 20px; border-radius: 5px; text-decoration: none; font-size: 16px;'>
                            Confirm My Email
                        </a>
                    </div>
                    <p style='font-size: 14px; color: #6c757d; text-align: center;'>
                        Or copy and paste this link into your browser:<br>
                        <a href='{confirmationLink}' style='color: #007bff;'>{confirmationLink}</a>
                    </p>"
                );

                await _emailSender.SendEmailAsync(email, subject, message);
                _logger.LogInformation($"Validation email sent successfully to {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send validation email to {email}: {ex.Message}");
                throw;
            }
        }

        public async Task SendEmailAfterChangePassAsync(string username, string email)
        {
            string subject = "Password Changed Notification - Secure Your Account";
            string message = CreateEmailTemplate(
                "Password Changed Notification",
                $@"
                <h1 style='color: #28a745; margin-bottom: 20px;'>Password Changed Successfully</h1>
                <p style='font-size: 16px; line-height: 1.6; color: #34495e;'>
                    Hello <strong>{username}</strong>,<br>
                    This is to inform you that your account password was recently changed.
                </p>"
            );
            await _emailSender.SendEmailAsync(email, subject, message);
        }

        public async Task SendPasswordResetEmailAsync(string email, string username, string token)
        {
            try
            {
                string subject = "Password Reset Request - Secure Your Account";
                var encodedEmail = WebUtility.UrlEncode(email);
                var resetLink = $"{_configuration["FrontEndUrl"]}/reset-password?email={encodedEmail}&token={token}";

                string message = CreateEmailTemplate(
                    "Password Reset Request",
                    $@"
                    <h1 style='color: #dc3545; margin-bottom: 20px;'>Password Reset Request</h1>
                    <p style='font-size: 16px; line-height: 1.6; color: #34495e;'>
                        Hello <strong>{username}</strong>,<br>
                        We received a request to reset your password. If this was you, click the button below to set a new password.
                    </p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{resetLink}' style='background-color: #dc3545; color: white; padding: 12px 20px; text-decoration: none; border-radius: 8px; font-size: 16px;'>
                            Reset Password
                        </a>
                    </div>"
                );

                await _emailSender.SendEmailAsync(email, subject, message);
                _logger.LogInformation($"Password reset email sent successfully to {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send password reset email to {email}");
                throw;
            }
        }

        public async Task SendPasswordResetSuccessEmailAsync(string email)
        {
            string subject = "Password Reset Successful - Account Secured";
            string message = CreateEmailTemplate(
                "Password Reset Successful",
                $@"<h1 style='color: #28a745; margin-bottom: 20px;'>Password Reset Successful</h1>
                   <p style='font-size: 16px; line-height: 1.6; color: #34495e;'>Your account is now secured with your new password.</p>"
            );
            await _emailSender.SendEmailAsync(email, subject, message);
        }

        public async Task SendAccountLockedEmailAsync(string email, string username, string reason = "Multiple failed login attempts")
        {
            string subject = "Account Locked - Security Alert";
            string message = CreateEmailTemplate(
                "Account Locked",
                $@"<h1 style='color: #dc3545; margin-bottom: 20px;'>Account Locked</h1>
                   <p>Hello <strong>{username}</strong>, your account has been temporarily locked. Reason: {reason}</p>"
            );
            await _emailSender.SendEmailAsync(email, subject, message);
        }

        public async Task SendWelcomeEmailAsync(string email, string username)
        {
            string subject = "Welcome to Our Service - Get Started!";
            string message = CreateEmailTemplate(
                "Welcome",
                $@"<h1 style='color: #28a745; margin-bottom: 20px;'>Welcome to Our Service!</h1>
                   <p>Hello <strong>{username}</strong>, welcome to our community!</p>"
            );
            await _emailSender.SendEmailAsync(email, subject, message);
        }

        private string CreateEmailTemplate(string title, string content)
        {
            return $@"
                <!DOCTYPE html>
                <html lang='en'>
                <head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title>{title}</title>
                </head>
                <body style='margin: 0; padding: 0; font-family: Arial, sans-serif; background-color: #f8f9fa;'>
                    <div style='max-width: 600px; margin: 0 auto; background-color: white; padding: 40px 20px;'>
                        <div style='text-align: center; margin-bottom: 30px;'>
                            <h2 style='color: #2c3e50; margin: 0; font-size: 24px;'>{title}</h2>
                        </div>
                        <div style='line-height: 1.6;'>{content}</div>
                    </div>
                </body>
                </html>";
        }
    }
}
