using ContentService.Domain.Entities;

namespace ContentService.Application.Interfaces;

public interface ICommentRepo : IRepositoryBase<Comment>
{
    Task<(int BlogId, int CommentIndex)> GetCommentIndex(int commentId);
}