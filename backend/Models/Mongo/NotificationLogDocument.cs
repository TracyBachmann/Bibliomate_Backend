using backend.Models.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models.Mongo
{
    /// <summary>
    /// MongoDB document for logging notification events sent to users.
    /// </summary>
    public class NotificationLogDocument
    {
        /// <summary>
        /// MongoDB ObjectId as string.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;  // Assigned by MongoDB driver

        /// <summary>
        /// Identifier of the user who received the notification.
        /// </summary>
        [BsonElement("userId")]
        public int UserId { get; set; }

        /// <summary>
        /// Type of notification (e.g., LoanReminder, ReservationAvailable).
        /// </summary>
        [BsonElement("type")]
        public NotificationType Type { get; set; }

        /// <summary>
        /// Content of the notification message.
        /// </summary>
        [BsonElement("message")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// UTC timestamp when the notification was sent.
        /// </summary>
        [BsonElement("sentAt")]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}