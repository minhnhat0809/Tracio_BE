using MediatR;
using UserService.Application.DTOs.Auths;
using UserService.Application.DTOs.ResponseModel;

namespace UserService.Application.Commands;

public class LoginCommand: IRequest<ResponseModel>
{
    public LoginRequestModel? LoginModel { get; set; }
}
