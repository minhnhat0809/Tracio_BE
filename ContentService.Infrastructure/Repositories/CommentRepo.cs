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
    public async Task<(int BlogId, int CommentIndex)> GetCommentIndex(int commentId)
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
                                     blog_id,
                                     ROW_NUMBER() OVER (ORDER BY created_at) AS RowNum
                                 FROM comment 
                                 WHERE blog_id = (SELECT blog_id FROM comment WHERE comment_id = @CommentId)
                             )
                             SELECT blog_id AS BlogId, RowNum - 1 AS CommentIndex 
                             FROM OrderedComments 
                             WHERE comment_id = @CommentId;
                           """;

        var parameters = new { CommentId = commentId };

        var result = await connection.QueryFirstOrDefaultAsync<(int BlogId, int CommentIndex)>(sql, parameters);

        return result == default ? (-1, -1) : result; // Return (-1, -1) if not found
    }


}