using RouteService.Application.Interfaces;
using RouteService.Domain.Entities;
using RouteService.Infrastructure.Contexts;

namespace RouteService.Infrastructure.Repositories;

public class ReactionRepository(TracioRouteDbContext context) : RepositoryBase<Reaction>(context), IReactionRepository
{
    
}