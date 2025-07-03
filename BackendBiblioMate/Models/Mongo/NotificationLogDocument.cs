using BackendBiblioMate.Models.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BackendBiblioMate.Models.Mongo
{
    /// <summary>
    /// Represents a MongoDB document logging each notification event sent to users.
    /// Provides details about the notification type, recipient, message content, and timestamp.
    /// </summary>
    public class NotificationLogDocument
    {
        /// <summary>
        /// Gets or sets the MongoDB ObjectId string that uniquely identifies this document.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        /// <summary>
        /// Gets or sets the identifier of the user who received the notification.
        /// </summary>
        [BsonElement("userId")]
        public int UserId { get; set; }

        /// <summary>
        /// Gets or sets the type of notification sent (e.g., ReservationAvailable, ReturnReminder).
        /// </summary>
        [BsonElement("type")]
        public NotificationType Type { get; set; }

        /// <summary>
        /// Gets or sets the content text of the notification message.
        /// </summary>
        [BsonElement("message")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the UTC timestamp indicating when the notification was sent.
        /// </summary>
        [BsonElement("sentAt")]
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}