using UserService.Domain.Entities;

namespace UserService.Application.Interfaces;

public interface IAuthRepository
{
    Task<(User? User, string IdToken, string RefreshToken)> HandleGoogleSignInWithTokensAsync(string idToken);
    Task<(User? User, string IdToken, string RefreshToken)> HandleEmailPasswordSignInWithTokensAsync(string email, string password);
}