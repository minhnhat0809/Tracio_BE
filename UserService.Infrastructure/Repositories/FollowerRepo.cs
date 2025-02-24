using UserService.Application.Interfaces;
using UserService.Domain.Entities;
using UserService.Infrastructure.Contexts;

namespace UserService.Infrastructure.Repositories;

public class FollowerRepo(TracioUserDbContext context) : RepositoryBase<Follower>(context), IFollowerRepo
{
    
}