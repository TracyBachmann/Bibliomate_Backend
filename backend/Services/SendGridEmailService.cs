using SendGrid;
using SendGrid.Helpers.Mail;

namespace backend.Services
{
    /// <summary>
    /// Service responsible for sending emails via SendGrid.
    /// </summary>
    public class SendGridEmailService : IEmailService
    {
        private readonly IConfiguration _config;

        /// <summary>
        /// Constructs the email service with application configuration.
        /// </summary>
        /// <param name="config">
        /// Configuration containing SendGrid settings (ApiKey, FromEmail, FromName).
        /// </param>
        public SendGridEmailService(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Sends a single email asynchronously using SendGrid.
        /// </summary>
        /// <param name="toEmail">Recipient email address.</param>
        /// <param name="subject">Subject line of the email.</param>
        /// <param name="htmlContent">HTML body content of the email.</param>
        /// <returns>
        /// A <see cref="Task"/> that completes once SendGrid responds.
        /// </returns>
        public async Task SendEmailAsync(string toEmail, string subject, string htmlContent)
        {
            // Retrieve SendGrid configuration values
            var apiKey    = _config["SendGrid:ApiKey"];
            var fromEmail = _config["SendGrid:FromEmail"];
            var fromName  = _config["SendGrid:FromName"];

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("SendGrid API key is not configured.");

            // Initialize the SendGrid client
            var client = new SendGridClient(apiKey);

            // Build email addresses
            var from = new EmailAddress(fromEmail, fromName);
            var to   = new EmailAddress(toEmail);

            // Create the email message
            var msg = MailHelper.CreateSingleEmail(
                from, 
                to, 
                subject, 
                plainTextContent: null, 
                htmlContent: htmlContent
            );

            // Send the email
            var response = await client.SendEmailAsync(msg);

            // Optional: log the response for diagnostics
            var responseBody = await response.Body.ReadAsStringAsync();
            Console.WriteLine($"[SENDGRID] Status: {response.StatusCode}");
            Console.WriteLine($"[SENDGRID] Response body: {responseBody}");
        }
    }
}
