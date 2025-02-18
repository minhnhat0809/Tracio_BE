using RouteService.Application.Interfaces;
using RouteService.Infrastructure.Contexts;
using RouteService.Infrastructure.Repositories;

namespace RouteService.Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    public IRouteRepository? RouteRepository { get; private set; }
    public UnitOfWork(TracioRouteDbContext context)
    {
        RouteRepository = new RouteRepository(context);
    }
    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public Task<int> SaveChangesAsync()
    {
        throw new NotImplementedException();
    }

    public Task BeginTransactionAsync()
    {
        throw new NotImplementedException();
    }

    public Task CommitTransactionAsync()
    {
        throw new NotImplementedException();
    }

    public Task RollbackTransactionAsync()
    {
        throw new NotImplementedException();
    }
}