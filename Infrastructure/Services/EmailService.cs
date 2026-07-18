using Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MailKit.Net.Smtp;
using MimeKit;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string htmlMessage)
        {
            var host = _configuration["SmtpSettings:Host"];
            var port = _configuration.GetValue<int>("SmtpSettings:Port");
            var userName = _configuration["SmtpSettings:UserName"];
            var password = _configuration["SmtpSettings:Password"];
            var from = _configuration["SmtpSettings:From"];

            if (string.IsNullOrEmpty(host) || port == 0 || string.IsNullOrEmpty(password) || password == "your-app-password" || (host != null && host.Equals("localhost", System.StringComparison.OrdinalIgnoreCase)))
            {
                // Fallback for local testing: Save email as HTML file in the project folder
                try
                {
                    var folderPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "sent-emails");
                    System.IO.Directory.CreateDirectory(folderPath);
                    
                    var safeSubject = string.Concat(subject.Split(System.IO.Path.GetInvalidFileNameChars())).Replace(" ", "_");
                    var fileName = $"{System.DateTime.Now:yyyyMMdd_HHmmss}_{to}_{safeSubject}.html";
                    var filePath = System.IO.Path.Combine(folderPath, fileName);
                    
                    var fileContent = $@"<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: sans-serif; margin: 20px; }}
        .header {{ background: #f5f5f5; padding: 15px; border-radius: 5px; margin-bottom: 20px; border: 1px solid #ddd; }}
        .header-item {{ margin: 5px 0; }}
        .label {{ font-weight: bold; color: #555; }}
    </style>
</head>
<body>
    <div class='header'>
        <div class='header-item'><span class='label'>To:</span> {to}</div>
        <div class='header-item'><span class='label'>Subject:</span> {subject}</div>
        <div class='header-item'><span class='label'>Sent (Local):</span> {System.DateTime.Now}</div>
    </div>
    <div class='content'>
        {htmlMessage}
    </div>
</body>
</html>";
                    
                    await System.IO.File.WriteAllTextAsync(filePath, fileContent);
                    _logger.LogInformation($"[EmailService] Email saved locally: file:///{filePath.Replace('\\', '/')}");
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex, "Failed to write mock email to local file");
                }

                _logger.LogInformation($"[EmailService] Mock Email to {to} | Subject: {subject}");
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
                _logger.LogError(ex, $"[EmailService] Failed to send email to {to}");
                throw new System.InvalidOperationException($"Email sending failed: {ex.Message}", ex);
            }
        }
    }
}

