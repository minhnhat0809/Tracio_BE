using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using ContentService.Infrastructure.Contexts;

namespace ContentService.Infrastructure.Repositories;

public class FollowerOnlyBlogRepo(TracioContentDbContext context) : RepositoryBase<UserBlogFollowerOnly>(context),IFollowerOnlyBlogRepo
{
    
}