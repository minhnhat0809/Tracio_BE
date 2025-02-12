using ContentService.Domain.Entities;

namespace ContentService.Application.Interfaces;

public interface ICommentRepo : IRepositoryBase<Comment>
{
    Task<bool> DeleteComment(int commentId);
    
    Task IncrementReplyCount(int commentId);
}