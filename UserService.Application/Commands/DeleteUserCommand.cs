namespace UserService.Application.Commands;
using MediatR;
using UserService.Application.DTOs.ResponseModel;

public class DeleteUserCommand : IRequest<ResponseModel>
{
    public int UserId { get; set; } 
}
