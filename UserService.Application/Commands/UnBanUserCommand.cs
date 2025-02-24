using MediatR;
using UserService.Application.DTOs.ResponseModel;

namespace UserService.Application.Commands;

public class UnBanUserCommand: IRequest<ResponseModel>
{
    public int UserId { get; set; } 
}
