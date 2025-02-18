using RouteService.Application.Interfaces;
using RouteService.Domain.Entities;
using RouteService.Infrastructure.Contexts;

namespace RouteService.Infrastructure.Repositories;

public class RouteRepository(TracioRouteDbContext context) : RepositoryBase<Route>(context), IRouteRepository
{
    
}
