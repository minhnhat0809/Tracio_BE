using System.Data;
using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using ContentService.Infrastructure.Contexts;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace ContentService.Infrastructure.Repositories;

public class CommentRepo(TracioContentDbContext context, IConfiguration configuration) : RepositoryBase<Comment>(context),ICommentRepo
{
    private readonly TracioContentDbContext _context = context;


    public async Task<int> GetCommentIndex(int blogId, int commentId)
    {
        var connectionString = configuration.GetConnectionString("tracio_content_db");

        await using var connection = new MySqlConnection(connectionString); 

        if (connection.State == ConnectionState.Closed)
        {
            await connection.OpenAsync();
        }
        
        const string sql = """
                           
                                       WITH OrderedComments AS (
                                           SELECT 
                                               comment_id, 
                                               ROW_NUMBER() OVER (ORDER BY created_at) AS RowNum
                                           FROM comment 
                                           WHERE blog_id = @BlogId
                                       )
                                       SELECT RowNum - 1 AS CommentIndex FROM OrderedComments WHERE comment_id = @CommentId;
                           """;

        var parameters =  new { BlogId = blogId, CommentId = commentId };

        var result = await connection.QueryFirstOrDefaultAsync<dynamic>(sql, parameters);

        if (result == null)
            return -1; // Comment not found

        // Extract value explicitly
        var index = (int)result.CommentIndex;

        return index;
    }
}