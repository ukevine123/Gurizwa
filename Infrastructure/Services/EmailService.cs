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

            var appUrl = _configuration["AppUrl"] ?? "http://localhost:5005";
            
            // Try to find the physical path to the logo to embed it directly
            var baseDir = System.IO.Directory.GetCurrentDirectory();
            var possibleLogoPaths = new[] {
                System.IO.Path.Combine(baseDir, "wwwroot", "Images", "guriza_logo.png"),
                System.IO.Path.Combine(baseDir, "Web", "wwwroot", "Images", "guriza_logo.png"),
                System.IO.Path.Combine(baseDir, "..", "Web", "wwwroot", "Images", "guriza_logo.png"),
                @"c:\Users\KEVINE\Downloads\DigitalLoanPlatform2\DigitalLoanPlatform2\Web\wwwroot\Images\guriza_logo.png"
            };

            string logoPhysicalPath = null;
            foreach (var path in possibleLogoPaths)
            {
                if (System.IO.File.Exists(path))
                {
                    logoPhysicalPath = path;
                    break;
                }
            }

            var logoSrc = $"{appUrl.TrimEnd('/')}/Images/guriza_logo.png"; // Fallback URL
            if (!string.IsNullOrEmpty(logoPhysicalPath))
            {
                // For the local HTML fallback, we use base64 so it displays instantly in the browser
                var logoBytes = System.IO.File.ReadAllBytes(logoPhysicalPath);
                logoSrc = "data:image/png;base64," + System.Convert.ToBase64String(logoBytes);
            }

            var formattedHtmlMessage = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <style>
        .email-content h3:first-child {{
            margin-top: 0;
        }}
    </style>
</head>
<body style=""margin: 0; padding: 0; background-color: #F4F7FE; font-family: 'Century Gothic', Arial, sans-serif; color: #1B2559;"">
    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""background-color: #F4F7FE; padding: 40px 20px;"">
        <tr>
            <td align=""center"">
                <table width=""100%"" cellpadding=""0"" cellspacing=""0"" border=""0"" style=""max-width: 600px; background-color: #FFFFFF; border-radius: 12px; box-shadow: 0px 4px 25px rgba(0, 0, 0, 0.05); overflow: hidden;"">
                    
                    <!-- Header -->
                    <tr>
                        <td align=""left"" style=""padding: 15px 30px; border-bottom: 1px solid #E2E8F0; background-color: #FFFFFF;"">
                            <img src=""{logoSrc}"" alt=""Guriza Logo"" style=""height: 45px; width: auto; max-width: 100%; display: block; border: 0; outline: none; text-decoration: none;"" />
                        </td>
                    </tr>

                    <!-- Body Content -->
                    <tr>
                        <td align=""left"" class=""email-content"" style=""padding: 15px 30px 30px 30px; font-size: 16px; line-height: 1.6; color: #1B2559;"">
                            {htmlMessage}
                            
                            <br><br>
                            <p style=""margin: 0;"">Best regards,<br><strong style=""color: #4318FF;"">The Guriza Team</strong></p>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td align=""center"" style=""background-color: #F4F7FE; padding: 20px; font-size: 13px; color: #A3AED0;"">
                            <p style=""margin: 0;"">&copy; {System.DateTime.Now.Year} Guriza. All rights reserved.</p>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";

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
        .local-header {{ background: #f5f5f5; padding: 15px; border-radius: 5px; margin-bottom: 20px; border: 1px solid #ddd; }}
        .header-item {{ margin: 5px 0; }}
        .label {{ font-weight: bold; color: #555; }}
    </style>
</head>
<body>
    <div class='local-header'>
        <div class='header-item'><span class='label'>To:</span> {to}</div>
        <div class='header-item'><span class='label'>Subject:</span> {subject}</div>
        <div class='header-item'><span class='label'>Sent (Local):</span> {System.DateTime.Now}</div>
    </div>
    <hr />
    {formattedHtmlMessage}
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

            var adminEmail = _configuration["AdminEmail"];
            if (!string.IsNullOrEmpty(adminEmail) && to != adminEmail)
            {
                email.Cc.Add(MailboxAddress.Parse(adminEmail));
            }

            email.Subject = subject;
            
            var builder = new BodyBuilder();
            
            // For actual SMTP emails, embed the image as a LinkedResource (CID) for best email client compatibility
            if (!string.IsNullOrEmpty(logoPhysicalPath))
            {
                var image = builder.LinkedResources.Add(logoPhysicalPath);
                
                // Use a proper RFC-compliant unique Message-ID for the Content-ID to prevent strict clients (like Gmail) from stripping it
                image.ContentId = MimeKit.Utils.MimeUtils.GenerateMessageId();
                image.IsAttachment = false; // Ensure it's treated as an inline image, not a downloadable attachment
                
                // Replace the base64 src with the correct cid: reference
                formattedHtmlMessage = formattedHtmlMessage.Replace(logoSrc, $"cid:{image.ContentId}");
            }
            
            builder.HtmlBody = formattedHtmlMessage;
            email.Body = builder.ToMessageBody();

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

