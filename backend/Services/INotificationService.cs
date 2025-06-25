namespace backend.Services
{
    /// <summary>
    /// Defines contract for sending notifications to users.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Sends a notification message to the specified user.
        /// </summary>
        /// <param name="userId">The identifier of the user to notify.</param>
        /// <param name="message">The content of the notification message.</param>
        Task NotifyUser(int userId, string message);
    }
}