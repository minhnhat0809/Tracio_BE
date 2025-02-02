using ContentService.Application.Interfaces;
using ContentService.Domain;
using ContentService.Domain.Entities;
using ContentService.Infrastructure.Contexts;

namespace ContentService.Infrastructure.Repositories;

public class ReactionRepo(TracioContentDbContext context) : RepositoryBase<Reaction>(context),IReactionRepo
{
    
}