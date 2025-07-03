using Microsoft.AspNetCore.SignalR;

namespace BackendBiblioMate.Hubs
{
    /// <summary>
    /// SignalR Hub for sending real-time notifications to connected clients.
    /// </summary>
    public class NotificationHub : Hub
    {
        /// <summary>
        /// Name of the client method to invoke when a notification is received.
        /// </summary>
        public const string ReceiveMethod = "ReceiveNotification";

        /// <summary>
        /// Broadcasts a notification message to all connected clients.
        /// </summary>
        /// <param name="message">The notification payload to send.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        public Task SendNotificationToAll(string message)
        {
            return Clients.All.SendAsync(ReceiveMethod, message);
        }

        /// <summary>
        /// Sends a notification message to a single user.
        /// </summary>
        /// <param name="userId">
        /// The target user's identifier (as returned by <c>Context.UserIdentifier</c>).
        /// </param>
        /// <param name="message">The notification payload to send.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        public Task SendNotificationToUser(string userId, string message)
        {
            return Clients.User(userId).SendAsync(ReceiveMethod, message);
        }

        /// <summary>
        /// Sends a notification message to all clients in a specific group.
        /// </summary>
        /// <param name="groupName">The name of the target group.</param>
        /// <param name="message">The notification payload to send.</param>
        /// <returns>A task that represents the asynchronous send operation.</returns>
        public Task SendNotificationToGroup(string groupName, string message)
        {
            return Clients.Group(groupName).SendAsync(ReceiveMethod, message);
        }

        /// <summary>
        /// (Optional) Adds the current connection to a specified group.
        /// </summary>
        /// <param name="groupName">The name of the group to join.</param>
        /// <returns>A task that represents the asynchronous group-add operation.</returns>
        public Task JoinGroup(string groupName)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        /// <summary>
        /// (Optional) Removes the current connection from a specified group.
        /// </summary>
        /// <param name="groupName">The name of the group to leave.</param>
        /// <returns>A task that represents the asynchronous group-remove operation.</returns>
        public Task LeaveGroup(string groupName)
        {
            return Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }
    }
}
