using ContentService.Application.Interfaces;
using ContentService.Domain;
using ContentService.Domain.Entities;
using ContentService.Infrastructure.Contexts;

namespace ContentService.Infrastructure.Repositories;

public class BlogRepo(TracioContentDbContext context) : RepositoryBase<Blog>(context), IBlogRepo
{

}