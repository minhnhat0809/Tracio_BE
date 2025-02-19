using ContentService.Domain.Entities;

namespace ContentService.Application.Interfaces;

public interface IFollowerOnlyBlogRepo : IRepositoryBase<UserBlogFollowerOnly>
{
    Task MarkBlogsAsReadAsync(int userId, List<int> blogIds);
}