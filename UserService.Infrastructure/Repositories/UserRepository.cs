using Microsoft.EntityFrameworkCore;
using UserService.Application.Interfaces;
using UserService.Domain;
using UserService.Domain.Entities;
using UserService.Infrastructure.Contexts;

namespace UserService.Infrastructure.Repositories;


public class UserRepository(TracioUserDbContext context) : RepositoryBase<User>(context), IUserRepository
{
    // Find a user by a specific property (email, phone, username, etc.)
    public async Task<User?> GetUserByPropertyAsync(string prop)
    {
        return await _context.Users
            .Where(u => u.Email == prop || u. FirebaseId== prop || u. PhoneNumber== prop  )
            .FirstOrDefaultAsync();
    }
    // Find a user by multiple specific property(email, phone, firebaseId)
    public async Task<User?> GetUserByMultiplePropertiesAsync(string? email, string? uId, string? phoneNumber)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrEmpty(email))
        {
            query = query.Where(u => u.Email == email);
        }
    
        if (!string.IsNullOrEmpty(uId))
        {
            query = query.Where(u => u.FirebaseId == uId);
        }
    
        if (!string.IsNullOrEmpty(phoneNumber))
        {
            query = query.Where(u => u.PhoneNumber == phoneNumber);
        }

        return await query.FirstOrDefaultAsync();
    }
    
    public async Task<bool> AreBothUsersExistAsync(int userId1, int userId2)
    {
        var count = await _context.Users
            .Where(u => u.UserId == userId1 || u.UserId == userId2)
            .CountAsync();

        return count == 2; 
    }

    public async Task<List<int>> GetFollowingsOfUser(int userId, List<int> authorIds)
    {
        return  await _context.Followers
            .Where(f => f.FollowerId == userId && authorIds.Contains(f.FollowedId))
            .Select(f => f.FollowedId)
            .ToListAsync();
    }
}