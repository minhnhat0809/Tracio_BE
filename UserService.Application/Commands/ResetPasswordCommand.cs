namespace UserService.Application.Commands;

using MediatR;
using UserService.Application.DTOs.ResponseModel;

public class ResetPasswordCommand : IRequest<ResponseModel>
{
    public string Email { get; set; }
}
