using ContentService.Domain.Entities;

namespace ContentService.Application.Interfaces;

public interface IBlogRepo : IRepositoryBase<Blog>
{
    Task<Blog?> GetBlogWithComments(string blogId, int pageSize, int pageNumber, bool isAscending);
}