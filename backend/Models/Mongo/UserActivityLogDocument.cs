using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace backend.Models.Mongo
{
    public class UserActivityLogDocument
    {
        [BsonId, BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        [BsonElement("userId")]
        public int UserId { get; set; }
        
        [BsonElement("action")]
        public string Action { get; set; } 
        
        [BsonElement("details")]
        public string? Details { get; set; } 
        
        [BsonElement("timestamp")]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}