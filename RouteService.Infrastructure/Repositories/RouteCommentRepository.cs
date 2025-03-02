using Microsoft.EntityFrameworkCore;
using RouteService.Application.Interfaces;
using RouteService.Domain.Entities;
using RouteService.Infrastructure.Contexts;

namespace RouteService.Infrastructure.Repositories;

public class RouteCommentRepository(TracioRouteDbContext context) : RepositoryBase<RouteComment>(context), IRouteCommentRepository
{
    
}