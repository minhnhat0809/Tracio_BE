using RouteService.Application.Interfaces;
using RouteService.Domain.Entities;
using RouteService.Infrastructure.Contexts;

namespace RouteService.Infrastructure.Repositories;

public class RouteBookmarkRepository(TracioRouteDbContext context) : RepositoryBase<RouteBookmark>(context), IRouteBookmarkRepository
{
    
}