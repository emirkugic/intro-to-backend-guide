using MongoDB.Driver;
using blog_website_api.Models;

namespace blog_website_api.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetConnectionString("MongoDb"));
            _database = client.GetDatabase("blog-webpage-database");
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("users");
        public IMongoCollection<Blog> Blogs => _database.GetCollection<Blog>("blogs");
        public IMongoCollection<Comment> Comments => _database.GetCollection<Comment>("comments");

    }
}
