using UserService.Domain.Entities;

namespace UserService.Application.Interfaces;

public interface IUserSessionRepository : IRepositoryBase<UserSession>
{
    Task<UserSession?> GetSessionByRefreshToken(string refreshToken);
    Task<UserSession?> RevokeSessionByRefreshToken(string refreshToken);
    Task<bool> RevokeAllSessionsForUser(int userId);
}