using BackendBiblioMate.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace BackendBiblioMate.Services.Infrastructure.External
{
    /// <summary>
    /// Provides an SMTP-based implementation of <see cref="IEmailService"/>.
    /// Sends emails using the MailKit library and SMTP credentials configured in appsettings.
    /// </summary>
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _cfg;
        private readonly ILogger<SmtpEmailService> _log;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmtpEmailService"/> class.
        /// </summary>
        /// <param name="cfg">Application configuration containing SMTP settings.</param>
        /// <param name="log">Logger for diagnostic and audit purposes.</param>
        public SmtpEmailService(IConfiguration cfg, ILogger<SmtpEmailService> log)
        {
            _cfg = cfg;
            _log = log;
        }

        /// <summary>
        /// Sends an email asynchronously via the configured SMTP server.
        /// </summary>
        /// <param name="toEmail">Recipient email address.</param>
        /// <param name="subject">Email subject line.</param>
        /// <param name="htmlContent">HTML content of the message body.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous send operation.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if required SMTP configuration values are missing.
        /// </exception>
        public async Task SendEmailAsync(string toEmail, string subject, string htmlContent)
        {
            var host     = _cfg["Smtp:Host"] 
                           ?? throw new InvalidOperationException("Missing SMTP configuration: Smtp:Host");
            var port     = int.TryParse(_cfg["Smtp:Port"], out var p) ? p : 587;
            var user     = _cfg["Smtp:Username"] 
                           ?? throw new InvalidOperationException("Missing SMTP configuration: Smtp:Username");
            var pass     = _cfg["Smtp:Password"] 
                           ?? throw new InvalidOperationException("Missing SMTP configuration: Smtp:Password");
            var fromMail = _cfg["Smtp:FromEmail"] ?? user;
            var fromName = _cfg["Smtp:FromName"] ?? "BiblioMate";

            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress(fromName, fromMail));
            msg.To.Add(MailboxAddress.Parse(toEmail));
            msg.Subject = subject;
            msg.Body = new TextPart("html") { Text = htmlContent };

            try
            {
                using var client = new SmtpClient();

                // Connect (default: STARTTLS). SecureSocketOptions.Auto could adapt if SSL is required.
                await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);

                await client.AuthenticateAsync(user, pass);
                await client.SendAsync(msg);
                await client.DisconnectAsync(true);

                _log.LogInformation("SMTP email successfully sent to {To}", toEmail);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Failed to send SMTP email to {To}", toEmail);
                throw; // Rethrow so upper layers know the email failed
            }
        }
    }
}
