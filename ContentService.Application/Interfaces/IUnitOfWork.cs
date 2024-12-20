using MongoDB.Driver;

namespace ContentService.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IMongoClient Client { get; }
    IClientSessionHandle Session { get; }

    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}