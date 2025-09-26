using Microsoft.AspNetCore.SignalR;

namespace BackendBiblioMate.Hubs
{
    /// <summary>
    /// SignalR Hub responsible for handling real-time notification delivery.
    /// </summary>
    /// <remarks>
    /// This hub allows the server to broadcast messages to:
    /// <list type="bullet">
    ///   <item><description>All connected clients</description></item>
    ///   <item><description>A specific authenticated user</description></item>
    ///   <item><description>A logical group of clients (e.g., librarians, admins, or a book club)</description></item>
    /// </list>
    /// It also exposes helper methods to join and leave groups dynamically.
    /// 
    /// Clients must implement a handler for <see cref="ReceiveMethod"/> to process incoming notifications.
    /// Example in JavaScript:
    /// <code>
    /// connection.on("ReceiveNotification", message => {
    ///     console.log("New notification:", message);
    /// });
    /// </code>
    /// </remarks>
    public class NotificationHub : Hub
    {
        /// <summary>
        /// Name of the client-side method that receives notifications.
        /// Clients must subscribe to this method to process notifications.
        /// </summary>
        public const string ReceiveMethod = "ReceiveNotification";

        /// <summary>
        /// Broadcasts a notification to all connected clients.
        /// </summary>
        /// <param name="message">The notification payload (e.g., text message, JSON string).</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous broadcast operation.</returns>
        /// <example>
        /// Server usage:
        /// <code>
        /// await _hubContext.Clients.All.SendAsync(NotificationHub.ReceiveMethod, "System maintenance at 10 PM.");
        /// </code>
        /// </example>
        public Task SendNotificationToAll(string message) =>
            Clients.All.SendAsync(ReceiveMethod, message);

        /// <summary>
        /// Sends a notification to a single user.
        /// </summary>
        /// <param name="userId">
        /// The identifier of the target user (must match <c>Context.UserIdentifier</c> on the client).
        /// </param>
        /// <param name="message">The notification payload to send.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous send operation.</returns>
        /// <example>
        /// Server usage:
        /// <code>
        /// await _hubContext.Clients.User(userId).SendAsync(NotificationHub.ReceiveMethod, "You have a new loan.");
        /// </code>
        /// </example>
        public Task SendNotificationToUser(string userId, string message) =>
            Clients.User(userId).SendAsync(ReceiveMethod, message);

        /// <summary>
        /// Sends a notification to all clients belonging to a specific group.
        /// </summary>
        /// <param name="groupName">The logical group name (e.g., "Admins" or "Zone42").</param>
        /// <param name="message">The notification payload to send.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous send operation.</returns>
        public Task SendNotificationToGroup(string groupName, string message) =>
            Clients.Group(groupName).SendAsync(ReceiveMethod, message);

        /// <summary>
        /// Adds the current connection to a specified group.
        /// Useful for organizing users into roles, zones, or custom topics.
        /// </summary>
        /// <param name="groupName">The group name to join.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous group-add operation.</returns>
        /// <remarks>
        /// Groups are dynamic and do not need prior registration.
        /// </remarks>
        public Task JoinGroup(string groupName) =>
            Groups.AddToGroupAsync(Context.ConnectionId, groupName);

        /// <summary>
        /// Removes the current connection from a specified group.
        /// </summary>
        /// <param name="groupName">The group name to leave.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous group-remove operation.</returns>
        public Task LeaveGroup(string groupName) =>
            Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
    }
}
