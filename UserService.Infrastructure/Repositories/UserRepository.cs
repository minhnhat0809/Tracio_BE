using Microsoft.EntityFrameworkCore;
using UserService.Application.Interfaces;
using UserService.Domain;
using UserService.Domain.Entities;

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

}