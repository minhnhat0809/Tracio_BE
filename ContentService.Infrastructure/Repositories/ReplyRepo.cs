using ContentService.Application.Interfaces;
using ContentService.Domain;
using ContentService.Domain.Entities;
using ContentService.Infrastructure.Contexts;

namespace ContentService.Infrastructure.Repositories;

public class ReplyRepo(TracioContentDbContext context) : RepositoryBase<Reply>(context),IReplyRepo
{
    private readonly TracioContentDbContext _context = context;

    public async Task<bool> DeleteReply(int replyId)
    {
        try
        {
            var reply = await GetByIdAsync(r => r.ReplyId == replyId, r => r);
            reply!.IsDeleted = true;
            
            return await _context.SaveChangesAsync() > 0;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}