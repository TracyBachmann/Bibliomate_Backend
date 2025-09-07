using BackendBiblioMate.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace BackendBiblioMate.Services.Infrastructure.External
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _cfg;
        private readonly ILogger<SmtpEmailService> _log;
        public SmtpEmailService(IConfiguration cfg, ILogger<SmtpEmailService> log)
        { _cfg = cfg; _log = log; }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlContent)
        {
            var host     = _cfg["Smtp:Host"] ?? throw new InvalidOperationException("Smtp:Host manquant");
            var port     = int.TryParse(_cfg["Smtp:Port"], out var p) ? p : 587;
            var user     = _cfg["Smtp:Username"] ?? throw new InvalidOperationException("Smtp:Username manquant");
            var pass     = _cfg["Smtp:Password"] ?? throw new InvalidOperationException("Smtp:Password manquant");
            var fromMail = _cfg["Smtp:FromEmail"] ?? user;
            var fromName = _cfg["Smtp:FromName"] ?? "BiblioMate";

            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress(fromName, fromMail));
            msg.To.Add(MailboxAddress.Parse(toEmail));
            msg.Subject = subject;
            msg.Body = new TextPart("html") { Text = htmlContent };

            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(user, pass);
            await client.SendAsync(msg);
            await client.DisconnectAsync(true);

            _log.LogInformation("SMTP email sent to {To}", toEmail);
        }
    }
}