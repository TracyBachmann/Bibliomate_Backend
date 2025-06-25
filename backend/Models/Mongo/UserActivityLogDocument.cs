using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models.Mongo
{
    /// <summary>
    /// MongoDB document representing a user’s activity log entry.
    /// </summary>
    public class UserActivityLogDocument
    {
        /// <summary>
        /// MongoDB ObjectId as a string.
        /// </summary>
        [BsonId, BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;  // Populated by MongoDB

        /// <summary>
        /// Identifier of the user who performed the action.
        /// </summary>
        [BsonElement("userId")]
        public int UserId { get; set; }

        /// <summary>
        /// Type of action performed (e.g., "CreateUser", "DeleteLoan").
        /// </summary>
        [BsonElement("action")]
        public string Action { get; set; } = null!;

        /// <summary>
        /// Optional details or metadata about the action.
        /// </summary>
        [BsonElement("details")]
        public string? Details { get; set; }

        /// <summary>
        /// UTC timestamp when the action occurred.
        /// </summary>
        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}