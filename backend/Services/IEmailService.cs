namespace backend.Services
{
    /// <summary>
    /// Contract for sending email messages.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends an email asynchronously.
        /// </summary>
        /// <param name="toEmail">Recipient email address.</param>
        /// <param name="subject">Subject line of the email.</param>
        /// <param name="htmlContent">HTML content of the email.</param>
        Task SendEmailAsync(string toEmail, string subject, string htmlContent);
    }
}