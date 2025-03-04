using RouteService.Application.DTOs.Users;

namespace RouteService.Application.Interfaces;

public interface IUserRepository
{
    Task<bool> ValidateUserAsync(int userId);
}