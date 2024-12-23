using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using MongoDB.Driver;

namespace ContentService.Infrastructure.Repositories;

public class ReactionRepo(IMongoDatabase database) : RepositoryBase<Reaction>(database, "reactions"), IReactionRepo
{
    
}