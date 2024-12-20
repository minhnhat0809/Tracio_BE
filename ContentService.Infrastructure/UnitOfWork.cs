using ContentService.Application.Interfaces;
using MongoDB.Driver;

namespace ContentService.Infrastructure;

public class UnitOfWork(IMongoClient client, string databaseName) : IUnitOfWork
{
    private IClientSessionHandle? _session;

    public IMongoClient Client => client;

    public IClientSessionHandle Session => _session ?? throw new InvalidOperationException("No active session. BeginTransactionAsync must be called first.");

    public async Task BeginTransactionAsync()
    {
        if (_session != null)
            throw new InvalidOperationException("A transaction is already in progress.");

        _session = await client.StartSessionAsync();
        _session.StartTransaction();
    }

    public async Task CommitTransactionAsync()
    {
        if (_session == null)
            throw new InvalidOperationException("No active session. BeginTransactionAsync must be called first.");

        await _session.CommitTransactionAsync();
        _session.Dispose();
        _session = null;
    }

    public async Task RollbackTransactionAsync()
    {
        if (_session == null)
            throw new InvalidOperationException("No active session. BeginTransactionAsync must be called first.");

        await _session.AbortTransactionAsync();
        _session.Dispose();
        _session = null;
    }

    public void Dispose()
    {
        _session?.Dispose();
        _session = null;
    }
}