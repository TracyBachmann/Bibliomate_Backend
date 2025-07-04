namespace BackendBiblioMate.Models.Enums
{
    /// <summary>
    /// Defines the categories of notifications that can be issued to users,
    /// such as reservation updates, reminders, penalties, and custom messages.
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// Indicates that a reserved item has become available for pickup.
        /// </summary>
        ReservationAvailable,

        /// <summary>
        /// A reminder that an item is due to be returned soon.
        /// </summary>
        ReturnReminder,

        /// <summary>
        /// Notification that a penalty has been applied for an overdue item.
        /// </summary>
        OverduePenalty,

        /// <summary>
        /// Notice that an item is currently overdue.
        /// </summary>
        OverdueNotice,

        /// <summary>
        /// A custom, application-defined notification.
        /// </summary>
        Custom,

        /// <summary>
        /// Indicates an error occurred in the system or process.
        /// </summary>
        Error,

        /// <summary>
        /// A warning about a non-critical issue or potential problem.
        /// </summary>
        Warning,

        /// <summary>
        /// General informational message to the user.
        /// </summary>
        Info,
    }
}