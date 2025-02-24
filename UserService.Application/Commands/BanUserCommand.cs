using MediatR;
using UserService.Application.DTOs.ResponseModel;
using UserService.Application.DTOs.Users;

namespace UserService.Application.Commands;

public class BanUserCommand() : IRequest<ResponseModel>
{
    public int UserId { get; set; } 
}
