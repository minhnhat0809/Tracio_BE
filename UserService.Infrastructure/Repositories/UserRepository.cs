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
    public async Task<User?> GetUserByMultiplePropertiesAsync(string email, string uId, string phoneNumber)
    {
        return await _context.Users
            .Where(u => u.Email == email || u.FirebaseId == uId || u.PhoneNumber  == phoneNumber  )
            .FirstOrDefaultAsync();
    }
}