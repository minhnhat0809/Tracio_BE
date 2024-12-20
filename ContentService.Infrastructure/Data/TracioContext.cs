using ContentService.Domain.Entities;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace ContentService.Infrastructure.Data
{
    public class TracioContext
    {
        private readonly IMongoDatabase _database;

        // Constructor initializes the MongoDB connection and selects the database
        public TracioContext(IConfiguration configuration)
        {
            var connectionString = configuration.GetSection("MongoDB:ConnectionString").Value;
            var databaseName = configuration.GetSection("MongoDB:DatabaseName").Value;

            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(databaseName);
        }
        
        public IMongoCollection<Blog> Blogs => _database.GetCollection<Blog>("blogs");

        public IMongoCollection<Comment> Comments => _database.GetCollection<Comment>("comments");

        public IMongoCollection<Reaction> Reactions => _database.GetCollection<Reaction>("reactions");

        public IMongoCollection<Reply> Replies => _database.GetCollection<Reply>("replies");
        
    }
}