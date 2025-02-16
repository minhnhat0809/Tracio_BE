using ContentService.Domain.Entities;

namespace ContentService.Application.Interfaces;

public interface IBlogRepo : IRepositoryBase<Blog>
{
    Task<bool> ArchiveBlog(int blogId);
    
    Task IncrementCommentCount(int blogId);
    
    Task IncrementReactionCount(int blogId);
    
    Task DecrementReactionCount(int blogId);
    
    Task DecrementCommentCount(int blogId);
}