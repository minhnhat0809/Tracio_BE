using Microsoft.EntityFrameworkCore;
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

    public async Task<UserSession?> GetSessionByRefreshToken(string refreshToken)
    {
        var session = await _context.UserSessions.FirstOrDefaultAsync(s =>s.RefreshToken == refreshToken);
       
        return session;
    }
    public async Task<UserSession?> RevokeSessionByRefreshToken(string refreshToken)
    {
        
        var session = await _context.UserSessions.FirstOrDefaultAsync(s => s.RefreshToken == refreshToken);

        if (session != null)
        {
            session.AccessToken = null;
            session.RefreshToken = null;
            session.ExpiresAt = DateTime.UtcNow;

            _context.UserSessions.Update(session);
        }

        await _context.SaveChangesAsync();

        return session; 
    }
    public async Task<bool> RevokeAllSessionsForUser(int userId)
    {
        var sessions = await _context.UserSessions
            .Where(s => s.UserId == userId)
            .ToListAsync(); // Fetch all user sessions first

        // expired > now
        var activeSessions = sessions.Where(s => s.ExpiresAt > DateTime.UtcNow).ToList();

        if (activeSessions.Any())
        {
            foreach (var session in activeSessions)
            {
                session.AccessToken = null;
                session.RefreshToken = null;
                session.ExpiresAt = DateTime.UtcNow; // Mark session as expired
            }

            _context.UserSessions.UpdateRange(activeSessions);
            await _context.SaveChangesAsync();
        }

        return true;
    }



}