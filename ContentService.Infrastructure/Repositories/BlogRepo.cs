using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using MongoDB.Driver;

namespace ContentService.Infrastructure.Repositories;

public class BlogRepo(IMongoDatabase database) : RepositoryBase<Blog>(database, "blogs"), IBlogRepo
{
    
}