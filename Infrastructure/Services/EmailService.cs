using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using MailKit.Net.Smtp;
using MimeKit;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string htmlMessage)
        {
            var host = _configuration["SmtpSettings:Host"];
            var port = _configuration.GetValue<int>("SmtpSettings:Port");
            var userName = _configuration["SmtpSettings:UserName"];
            var password = _configuration["SmtpSettings:Password"];
            var from = _configuration["SmtpSettings:From"];

            if (string.IsNullOrEmpty(host) || port == 0 || string.IsNullOrEmpty(password) || password == "your-app-password")
            {
                // Fallback for development if SMTP is not configured
                System.Console.WriteLine($"[EmailService] Mock Email to {to} | Subject: {subject}");
                System.Console.WriteLine(htmlMessage);
                return;
            }

            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(from));
            email.To.Add(MailboxAddress.Parse(to));

            // Also send a copy to the developer email (adminEmail)
            var adminEmail = _configuration["AdminEmail"];
            if (!string.IsNullOrEmpty(adminEmail) && to != adminEmail)
            {
                email.Cc.Add(MailboxAddress.Parse(adminEmail));
            }

            email.Subject = subject;
            email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = htmlMessage };

            try
            {
                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(userName, password);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"[EmailService] Failed to send email to {to}. Error: {ex.Message}");
                // Not throwing here so registration doesn't fail if SMTP credentials are bad during test.
            }
        }
    }
}
