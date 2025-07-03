using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BackendBiblioMate.Models.Mongo
{
    /// <summary>
    /// Represents a MongoDB document logging various user activities within the application.
    /// Captures the user, action performed, optional details, and timestamp of the activity.
    /// </summary>
    public class UserActivityLogDocument
    {
        /// <summary>
        /// Gets the MongoDB ObjectId string that uniquely identifies this document.
        /// </summary>
        [BsonId, BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; init; } = null!;

        /// <summary>
        /// Gets the identifier of the user who performed the logged action.
        /// </summary>
        [BsonElement("userId")]
        public int UserId { get; init; }

        /// <summary>
        /// Gets the action type performed by the user (e.g., "CreateUser", "DeleteLoan").
        /// </summary>
        [BsonElement("action")]
        public string Action { get; init; } = null!;

        /// <summary>
        /// Gets optional metadata or details about the action. Null if none provided.
        /// </summary>
        [BsonElement("details")]
        public string? Details { get; init; }

        /// <summary>
        /// Gets the UTC timestamp indicating when the action was performed.
        /// </summary>
        [BsonElement("timestamp")]
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    }
}