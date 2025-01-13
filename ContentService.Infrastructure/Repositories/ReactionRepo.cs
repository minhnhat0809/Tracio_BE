using ContentService.Application.Interfaces;
using ContentService.Domain;
using ContentService.Domain.Entities;

namespace ContentService.Infrastructure.Repositories;

public class ReactionRepo(TracioContentDbContext context) : RepositoryBase<Reaction>(context),IReactionRepo
{
    
}