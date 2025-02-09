using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using ContentService.Infrastructure.Contexts;

namespace ContentService.Infrastructure.Repositories;

public class CategoryRepo (TracioContentDbContext context) : RepositoryBase<BlogCategory>(context), ICategoryRepo
{
    
}