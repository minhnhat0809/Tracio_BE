using System.Data;
using ContentService.Application.Interfaces;
using ContentService.Domain;
using ContentService.Domain.Entities;
using ContentService.Infrastructure.Contexts;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace ContentService.Infrastructure.Repositories;

public class ReplyRepo(TracioContentDbContext context, IConfiguration configuration) : RepositoryBase<Reply>(context),IReplyRepo
{
    private readonly TracioContentDbContext _context = context;

    public async Task<(int CommentId, int ReplyIndex, int ReReplyId)> GetReplyIndex(int replyId)
    {
        var connectionString = configuration.GetConnectionString("tracio_content_db");

        await using var connection = new MySqlConnection(connectionString);

        if (connection.State == ConnectionState.Closed)
        {
            await connection.OpenAsync();
        }

        const string sql = """
                             WITH OrderedReplies AS (
                                 SELECT 
                                     reply_id, 
                                     comment_id,
                                     re_reply_id,
                                     ROW_NUMBER() OVER (ORDER BY created_at) AS RowNum
                                 FROM reply 
                                 WHERE comment_id = (SELECT comment_id FROM reply WHERE reply_id = @ReplyId)
                             )
                             SELECT comment_id AS CommentId, RowNum - 1 AS ReplyIndex, re_reply_id AS ReReplyId
                             FROM OrderedReplies 
                             WHERE reply_id = @ReplyId;
                           """;

        var parameters = new { ReplyId = replyId };

        var result = await connection.QueryFirstOrDefaultAsync<(int CommentId, int ReplyIndex, int ReReplyId)>(sql, parameters);

        return result == default ? (-1, -1, -1) : result; // Return (-1, -1) if not found
    }
}