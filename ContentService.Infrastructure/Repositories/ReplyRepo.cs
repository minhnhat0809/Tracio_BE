using ContentService.Application.Interfaces;
using ContentService.Domain;
using ContentService.Domain.Entities;
using ContentService.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;

namespace ContentService.Infrastructure.Repositories;

public class ReplyRepo(TracioContentDbContext context) : RepositoryBase<Reply>(context),IReplyRepo
{
    private readonly TracioContentDbContext _context = context;

    public async Task<bool> DeleteReply(int replyId)
    {
        try
        {
            var updatedRows = await _context.Replies
                .Where(r => r.ReplyId == replyId)
                .ExecuteUpdateAsync(b => b.SetProperty(x => x.IsDeleted, true));

            return updatedRows > 0;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}