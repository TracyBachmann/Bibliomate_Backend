namespace BackendBiblioMate.Interfaces
{
    /// <summary>
    /// High-level contract for sending notifications to users.
    /// This service abstracts the delivery mechanism (e.g. email, SMS, in-app, SignalR)
    /// and ensures messages are dispatched reliably.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Sends a notification message to a specific user.
        /// The implementation may decide the delivery channel(s)
        /// based on user preferences or system configuration.
        /// </summary>
        /// <param name="userId">
        /// The identifier of the user to notify.
        /// Must correspond to a valid user in the system.
        /// </param>
        /// <param name="message">
        /// The notification content to send.
        /// This may be plain text or a structured message, 
        /// depending on the implementation.
        /// </param>
        /// <param name="cancellationToken">
        /// A token used to observe cancellation requests during the operation.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous send operation.
        /// The task completes successfully once the notification has been dispatched,
        /// or faults if delivery fails.
        /// </returns>
        Task NotifyUser(
            int userId,
            string message,
            CancellationToken cancellationToken = default);
    }
}