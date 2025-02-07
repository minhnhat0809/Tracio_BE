namespace UserService.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    // repository
    // user
    IUserRepository UserRepository { get; }
    IUserSessionRepository UserSessionRepository { get; }
    
    IFirebaseStorageRepository FirebaseStorageRepository { get; }
    
    //
    Task<int> SaveChangesAsync();
    
    Task BeginTransactionAsync();
    
    Task CommitTransactionAsync();
    
    Task RollbackTransactionAsync();
}