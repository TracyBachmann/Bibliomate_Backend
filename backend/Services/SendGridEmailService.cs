using SendGrid;
using SendGrid.Helpers.Mail;

namespace backend.Services
{
    public class SendGridEmailService
    {
        private readonly IConfiguration _config;

        public SendGridEmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlContent)
        {
            var apiKey = _config["SendGrid:ApiKey"];
            var fromEmail = _config["SendGrid:FromEmail"];
            var fromName = _config["SendGrid:FromName"];

            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(fromEmail, fromName);
            var to = new EmailAddress(toEmail);

            var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlContent);
            var response = await client.SendEmailAsync(msg);
            
            var responseBody = await response.Body.ReadAsStringAsync();
            Console.WriteLine($"[SENDGRID] Status: {response.StatusCode}");
            Console.WriteLine($"[SENDGRID] Response body: {responseBody}");
            
        }
    }
}