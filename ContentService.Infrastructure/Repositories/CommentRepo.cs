using ContentService.Application.Interfaces;
using ContentService.Domain;
using ContentService.Domain.Entities;
using ContentService.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace ContentService.Infrastructure.Repositories;

public class CommentRepo(TracioContentDbContext context) : RepositoryBase<Comment>(context),ICommentRepo
{
    private readonly TracioContentDbContext _context = context;

    public async Task<bool> DeleteComment(int commentId)
    {
        try
        {
            var updatedRows = await _context.Comments
                .Where(c => c.CommentId == commentId)
                .ExecuteUpdateAsync(b => b.SetProperty(x => x.IsDeleted, true));

            return updatedRows > 0;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}