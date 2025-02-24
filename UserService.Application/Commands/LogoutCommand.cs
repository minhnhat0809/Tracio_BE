using MediatR;
using UserService.Application.DTOs.Auths;
using UserService.Application.DTOs.ResponseModel;

namespace UserService.Application.Commands;

public class LogoutCommand : IRequest<ResponseModel>
{
    public RefreshTokenRequestModel RequestModel { get; set; } = null!; 
}
