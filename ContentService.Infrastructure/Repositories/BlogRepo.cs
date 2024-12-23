using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ContentService.Infrastructure.Repositories;

public class BlogRepo(IMongoDatabase database) : RepositoryBase<Blog>(database, "blogs"), IBlogRepo
{
    public async Task<Blog?> GetBlogWithComments(string blogId, int pageSize, int pageNumber, bool isAscending)
{
    try
    {
        var pipeline = new[]
        {
            // Match the blog by its ID
            new BsonDocument("$match", new BsonDocument("_id", new ObjectId(blogId))),
            
            // Lookup to join comments
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "comments" },
                { "localField", "_id" },
                { "foreignField", "entity_id" },
                { "as", "comments" }
            }),

            // Break comments for sorting
            new BsonDocument("$unwind", new BsonDocument("path", "$comments")),

            // Sort comments 
            new BsonDocument("$sort", new BsonDocument("comments.created_at", isAscending ? 1 : -1)),
            
            // Group comments
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", "$_id" },
                { "user_id", new BsonDocument("$first", "$user_id") },
                { "tittle", new BsonDocument("$first", "$tittle") },
                { "content", new BsonDocument("$first", "$content") },
                { "created_at", new BsonDocument("$first", "$created_at") },
                { "likes_count", new BsonDocument("$first", "$likes_count") },
                { "comments_count", new BsonDocument("$first", "$comments_count") },
                { "comments", new BsonDocument("$push", "$comments") }
            }),

            // pagination
            new BsonDocument("$project", new BsonDocument
            {
                { "user_id", 1 },
                { "tittle", 1 },
                { "content", 1 },
                { "created_at", 1 },
                { "likes_count", 1 },
                { "comments_count", 1 },
                { "comments", new BsonDocument("$slice", new BsonArray
                    {
                        "$comments",
                        (pageNumber - 1) * pageSize,
                        pageSize
                    })
                }
            })
        };

        var blogWithComments = await Collection.Aggregate<Blog>(pipeline).FirstOrDefaultAsync();

        return blogWithComments;
    }
    catch (Exception e)
    {
        throw new Exception($"Failed to get blog with id: {blogId}", e);
    }
}

}