using DomainLayer.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace Bags_Shop_API.Services.Shared
{
    public class EmailSender : IEmailSender
	{
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private Email GetEmailConfig()
        {
            return new Email
            {
                Address = _configuration["Email:Address"] ?? throw new Exception("Can't Find Email address"),
                Password = _configuration["Email:Password"] ?? throw new Exception("Can't Find Email password"),
                Host = _configuration["Email:Host"] ?? throw new Exception("Can't Find Email host"),
                Port = int.Parse(_configuration["Email:Port"] ?? throw new Exception("Can't Find Email port"))
            };
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            Email config = GetEmailConfig();
            MailMessage mailMessage = new MailMessage
            {
                From = new MailAddress(config.Address),
                Subject = subject,
                Body = $"<html><body>{htmlMessage}</body></html>",
                IsBodyHtml = true,
            };
            mailMessage.To.Add(email);

            try
            {
                using (SmtpClient smtpClient = new SmtpClient(config.Host, config.Port))
                {
                    smtpClient.Credentials = new NetworkCredential(config.Address, config.Password);
                    smtpClient.EnableSsl = true;
                    await smtpClient.SendMailAsync(mailMessage);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to send email.", ex);
            }
        }
    }
}
