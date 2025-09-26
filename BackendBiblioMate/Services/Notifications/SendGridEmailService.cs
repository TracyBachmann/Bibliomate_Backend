using SendGrid;
using SendGrid.Helpers.Mail;
using BackendBiblioMate.Interfaces;

namespace BackendBiblioMate.Services.Notifications
{
    /// <summary>
    /// Email service implementation that uses <see cref="SendGridClient"/>
    /// to send transactional and notification emails.
    /// </summary>
    public class SendGridEmailService : IEmailService
    {
        private readonly string _apiKey;
        private readonly EmailAddress _fromAddress;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendGridEmailService"/> class.
        /// </summary>
        /// <param name="config">
        /// Application configuration containing SendGrid settings:
        /// <list type="bullet">
        ///   <item><c>SendGrid:ApiKey</c> – API key for authenticating with SendGrid.</item>
        ///   <item><c>SendGrid:FromEmail</c> – Default sender email address.</item>
        ///   <item><c>SendGrid:FromName</c> – Optional display name for the sender.</item>
        /// </list>
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if <paramref name="config"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown if any required SendGrid setting is missing or invalid.
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
        /// Sends an email asynchronously using the SendGrid API.
        /// </summary>
        /// <param name="toEmail">The recipient's email address.</param>
        /// <param name="subject">The subject line of the email.</param>
        /// <param name="htmlContent">The HTML-formatted content of the email body.</param>
        /// <returns>
        /// A <see cref="Task"/> that completes once the SendGrid API responds to the request.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="toEmail"/> or <paramref name="htmlContent"/> is <c>null</c>, empty, or whitespace.
        /// </exception>
        public async Task SendEmailAsync(string toEmail, string subject, string htmlContent)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentException("Recipient email must be provided.", nameof(toEmail));
            if (string.IsNullOrWhiteSpace(htmlContent))
                throw new ArgumentException("Email content must be provided.", nameof(htmlContent));

            // Step 1: Initialize SendGrid client
            var client = new SendGridClient(_apiKey);

            // Step 2: Prepare email message
            var to  = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(
                from:             _fromAddress,
                to:               to,
                subject:          subject,
                plainTextContent: null,        // Only HTML content is used here
                htmlContent:      htmlContent);

            // Step 3: Send the email and await the response
            var response = await client.SendEmailAsync(msg);

            // Step 4: Log response details for diagnostics (optional, replaceable with ILogger)
            using var responseBodyStream = await response.Body.ReadAsStreamAsync();
            using var reader             = new StreamReader(responseBodyStream);
            var responseBody             = await reader.ReadToEndAsync();

            Console.WriteLine($"[SendGrid] Status: {response.StatusCode}");
            Console.WriteLine($"[SendGrid] Response: {responseBody}");
        }
    }
}
