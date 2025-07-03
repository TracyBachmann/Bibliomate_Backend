namespace BackendBiblioMate.Models.Enums
{
    /// <summary>
    /// Types of notifications that can be sent to users regarding their reservations and account status.
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// Notification that a reserved item is now available for pickup.
        /// </summary>
        ReservationAvailable,

        /// <summary>
        /// Reminder to return an item soon before the due date.
        /// </summary>
        ReturnReminder,

        /// <summary>
        /// Notification of a penalty applied for an overdue item.
        /// </summary>
        OverduePenalty,

        /// <summary>
        /// Notice that an item is overdue.
        /// </summary>
        OverdueNotice,

        /// <summary>
        /// Custom notification type defined by specific application logic.
        /// </summary>
        Custom,

        /// <summary>
        /// Error notification indicating a failure occurred.
        /// </summary>
        Error,

        /// <summary>
        /// Warning notification indicating a non-critical issue.
        /// </summary>
        Warning,

        /// <summary>
        /// Informational notification for general messages.
        /// </summary>
        Info,
    }
}