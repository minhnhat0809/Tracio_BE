namespace UserService.Application.Interfaces;

public interface IUnitOfWork
{
    // repository
    // user
    IUserRepository UserRepository { get; }
    IUserSessionRepository UserSessionRepository { get; }
    
    IFirebaseStorageRepository FirebaseStorageRepository { get; }
    
}