using SendGrid;
using SendGrid.Helpers.Mail;
using BackendBiblioMate.Interfaces;

namespace BackendBiblioMate.Services.Notifications
{
    /// <summary>
    /// Service responsible for sending emails via SendGrid.
    /// </summary>
    public class SendGridEmailService : IEmailService
    {
        private readonly string _apiKey;
        private readonly EmailAddress _fromAddress;

        /// <summary>
        /// Initializes a new instance of <see cref="SendGridEmailService"/>.
        /// </summary>
        /// <param name="config">
        /// Application configuration containing SendGrid settings:
        /// <c>SendGrid:ApiKey</c>, <c>SendGrid:FromEmail</c>, <c>SendGrid:FromName</c>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="config"/> is null.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if any required SendGrid setting is missing or empty.
        /// </exception>
        public SendGridEmailService(IConfiguration config)
        {
            if (config is null) throw new ArgumentNullException(nameof(config));

            _apiKey = config["SendGrid:ApiKey"] 
                ?? throw new InvalidOperationException("SendGrid API key is not configured.");

            var fromEmail = config["SendGrid:FromEmail"];
            var fromName  = config["SendGrid:FromName"];
            if (string.IsNullOrWhiteSpace(fromEmail))
                throw new InvalidOperationException("SendGrid FromEmail is not configured.");

            _fromAddress = new EmailAddress(fromEmail, fromName);
        }

        /// <summary>
        /// Sends an email asynchronously using SendGrid.
        /// </summary>
        /// <param name="toEmail">Recipient email address.</param>
        /// <param name="subject">Subject line of the email.</param>
        /// <param name="htmlContent">HTML body content of the email.</param>
        /// <returns>Asynchronous task that completes once SendGrid responds.</returns>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="toEmail"/> or <paramref name="htmlContent"/> is null or empty.
        /// </exception>
        public async Task SendEmailAsync(string toEmail, string subject, string htmlContent)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentException("Recipient email must be provided.", nameof(toEmail));
            if (string.IsNullOrWhiteSpace(htmlContent))
                throw new ArgumentException("Email content must be provided.", nameof(htmlContent));

            var client = new SendGridClient(_apiKey);
            var to      = new EmailAddress(toEmail);
            var msg     = MailHelper.CreateSingleEmail(
                _fromAddress,
                to,
                subject,
                plainTextContent: null,
                htmlContent:      htmlContent);

            var response = await client.SendEmailAsync(msg);

            // Optionally log for diagnostics
            using var responseBodyStream = await response.Body.ReadAsStreamAsync();
            using var reader = new StreamReader(responseBodyStream);
            var responseBody = await reader.ReadToEndAsync();

            Console.WriteLine($"[SendGrid] Status: {response.StatusCode}");
            Console.WriteLine($"[SendGrid] Response: {responseBody}");
        }
    }
}