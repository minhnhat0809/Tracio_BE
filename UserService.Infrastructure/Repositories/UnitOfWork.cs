using Microsoft.Extensions.Configuration;
using UserService.Application.Interfaces;
using UserService.Domain;

namespace UserService.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    public IUserRepository? UserRepository { get; private set; }
    public IUserSessionRepository? UserSessionRepository { get; private set; }
    
    public IFirebaseStorageRepository? FirebaseStorageRepository { get; private set; }
    
    public UnitOfWork(TracioUserDbContext context)
    {
        UserRepository = new UserRepository(context);
        UserSessionRepository = new UserSessionRepository(context);
        FirebaseStorageRepository = new FirebaseStorageRepository();
    }
    
}