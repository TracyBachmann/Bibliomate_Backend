using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models.Mongo
{
    public class SearchActivityLogDocument
    {
        /// <summary>
        /// MongoDB ObjectId as string.
        /// </summary>
        [BsonId, BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;

        /// <summary>
        /// Identifier of the user who made the query. Null if anonymous.
        /// </summary>
        [BsonElement("userId")]
        public int? UserId { get; set; }

        /// <summary>
        /// Text of the search query executed by the user.
        /// </summary>
        [BsonElement("queryText")]
        public string QueryText { get; set; } = string.Empty;

        /// <summary>
        /// UTC timestamp when this entry was created.
        /// </summary>
        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}