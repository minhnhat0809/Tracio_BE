using ContentService.Domain.Entities;

namespace ContentService.Application.Interfaces;

public interface IReplyRepo : IRepositoryBase<Reply>
{
    Task<bool> DeleteReply(int replyId);
    
    Task IncrementReactionCount(int replyId);
    
    Task DecrementReactionCount(int replyId);
}