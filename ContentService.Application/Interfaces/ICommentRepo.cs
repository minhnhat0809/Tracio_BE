using ContentService.Domain.Entities;

namespace ContentService.Application.Interfaces;

public interface ICommentRepo : IRepositoryBase<Comment>
{
    Task<int> GetCommentIndex(int blogId, int commentId);
}