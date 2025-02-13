using UserService.Application.Interfaces;
using UserService.Domain;
using UserService.Domain.Entities;
using UserService.Infrastructure.Contexts;

namespace UserService.Infrastructure.Repositories;



public class UserSessionRepository : RepositoryBase<UserSession>, IUserSessionRepository
{
    public UserSessionRepository(TracioUserDbContext context) : base(context)
    {
    }
}