using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace blog_website_api.Models
{
    public class Comment
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("blogId")]
        public required string BlogId { get; set; }

        [BsonElement("userId")]
        public required string UserId { get; set; }

        [BsonElement("content")]
        public required string Content { get; set; }

        [BsonElement("datePosted")]
        public DateTime DatePosted { get; set; }
    }
}
