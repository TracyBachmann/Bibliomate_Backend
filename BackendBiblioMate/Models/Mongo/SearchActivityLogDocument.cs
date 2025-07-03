using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BackendBiblioMate.Models.Mongo
{
    /// <summary>
    /// Represents a MongoDB document logging user search activities within the application.
    /// Captures information about who performed the search, the query text, and when it occurred.
    /// </summary>
    public class SearchActivityLogDocument
    {
        /// <summary>
        /// Gets the MongoDB ObjectId string that uniquely identifies this document.
        /// </summary>
        [BsonId, BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; init; } = null!;

        /// <summary>
        /// Gets the identifier of the user who executed the search query.
        /// Null indicates an anonymous search.
        /// </summary>
        [BsonElement("userId")]
        public int? UserId { get; init; }

        /// <summary>
        /// Gets the text of the search query entered by the user.
        /// </summary>
        [BsonElement("queryText")]
        public string QueryText { get; init; } = string.Empty;

        /// <summary>
        /// Gets the UTC timestamp indicating when the search query was executed.
        /// </summary>
        [BsonElement("timestamp")]
        public DateTime Timestamp { get; init; } = DateTime.UtcNow;
    }
}