using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using MongoDB.Driver;

namespace ContentService.Infrastructure.Repositories;

public class CommentRepo(IMongoDatabase database) : RepositoryBase<Comment>(database, "comments"), ICommentRepo
{
    
}