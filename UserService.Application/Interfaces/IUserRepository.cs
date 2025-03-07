﻿using UserService.Domain.Entities;

namespace UserService.Application.Interfaces;

public interface IUserRepository : IRepositoryBase<User>
{
    Task<User?> GetUserByPropertyAsync(string prop);
    Task<User?> GetUserByMultiplePropertiesAsync(string email, string uId, string? phoneNumber);

}