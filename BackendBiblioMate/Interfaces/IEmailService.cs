namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// Contract for sending email messages.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends an email message asynchronously.
        /// </summary>
        /// <param name="toEmail">
        /// The recipient’s email address.
        /// </param>
        /// <param name="subject">
        /// The subject line of the email.
        /// </param>
        /// <param name="htmlContent">
        /// The HTML body content of the email.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that completes when the email has been sent.
        /// </returns>
        Task SendEmailAsync(
            string toEmail,
            string subject,
            string htmlContent);
    }
}