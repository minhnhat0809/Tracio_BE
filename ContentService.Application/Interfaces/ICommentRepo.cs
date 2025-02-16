using ContentService.Domain.Entities;

namespace ContentService.Application.Interfaces;

public interface ICommentRepo : IRepositoryBase<Comment>
{
    Task<bool> DeleteComment(int commentId);
    
    Task IncrementReplyCount(int commentId);
    
    Task IncrementReactionCount(int commentId);
    
    Task DecrementReplyCount(int commentId);
    
    Task DecrementReactionCount(int commentId);
}