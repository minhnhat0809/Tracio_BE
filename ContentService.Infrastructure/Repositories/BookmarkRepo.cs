using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using ContentService.Infrastructure.Contexts;

namespace ContentService.Infrastructure.Repositories;

public class BookmarkRepo(TracioContentDbContext context) : RepositoryBase<BlogBookmark>(context), IBookmarkRepo
{
    
}