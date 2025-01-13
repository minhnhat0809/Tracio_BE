using ContentService.Application.Interfaces;
using ContentService.Domain;
using ContentService.Domain.Entities;

namespace ContentService.Infrastructure.Repositories;

public class BlogRepo(TracioContentDbContext context) : RepositoryBase<Blog>(context), IBlogRepo
{

}