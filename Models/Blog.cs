using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace blog_website_api.Models
{
    public class Blog
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("title")]
        public required string Title { get; set; }

        [BsonElement("description")]
        public required string Description { get; set; }

        [BsonElement("dateCreated")]
        public DateTime DateCreated { get; set; }

        [BsonElement("imageUrl")]
        public required string ImageUrl { get; set; }

        [BsonElement("imageDeleteHash")]
        public required string ImageDeleteHash { get; set; }

        [BsonElement("userId")]
        public required string UserId { get; set; }
    }
}
