namespace NotificationService.Application.Interfaces;

public interface IUserService
{
    Task<bool> CheckUserValid(int user);
}