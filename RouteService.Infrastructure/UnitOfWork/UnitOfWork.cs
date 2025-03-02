using RouteService.Application.Interfaces;
using RouteService.Infrastructure.Contexts;
using RouteService.Infrastructure.Repositories;

namespace RouteService.Infrastructure.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    public IRouteRepository? RouteRepository { get; private set; }
    public IRouteBookmarkRepository RouteBookmarkRepository { get; }
    public IRouteCommentRepository RouteCommentRepository { get; }
    public IRouteMediaFileRepository RouteMediaFileRepository { get; }
    public IReactionRepository ReactionRepository { get; }
    public IFirebaseStorageRepository FirebaseStorageRepository { get; }
    public UnitOfWork(TracioRouteDbContext context)
    {
        FirebaseStorageRepository = new FirebaseStorageRepository();
        RouteBookmarkRepository = new RouteBookmarkRepository(context);
        RouteCommentRepository = new RouteCommentRepository(context);
        RouteMediaFileRepository = new RouteMediaFileRepository(context);
        ReactionRepository = new ReactionRepository(context);
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