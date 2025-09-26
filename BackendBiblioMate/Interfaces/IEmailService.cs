namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// Defines a contract for sending email messages,
    /// typically used for account verification, password reset,
    /// notifications, and system alerts.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends an email message asynchronously to a single recipient.
        /// </summary>
        /// <param name="toEmail">
        /// The recipient’s email address. Must be a valid email format.
        /// </param>
        /// <param name="subject">
        /// The subject line of the email.
        /// </param>
        /// <param name="htmlContent">
        /// The HTML body content of the email.  
        /// Supports inline formatting and links.
        /// </param>
        /// <returns>
        /// A task that completes when the email has been sent.  
        /// May throw exceptions if delivery fails (e.g. SMTP connection issues).
        /// </returns>
        Task SendEmailAsync(
            string toEmail,
            string subject,
            string htmlContent);
    }
}