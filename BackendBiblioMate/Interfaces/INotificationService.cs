using System.Threading;
using System.Threading.Tasks;

namespace BackendBiblioMate.Interfaces
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
        /// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
        /// <returns>
        /// A <see cref="Task"/> that completes when the notification has been sent.
        /// </returns>
        Task NotifyUser(
            int userId,
            string message,
            CancellationToken cancellationToken = default);
    }
}