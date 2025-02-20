namespace UserService.Application.Queries;

using MediatR;
using UserService.Application.DTOs.Auths;
using UserService.Application.DTOs.ResponseModel;

public class LoginQuery : IRequest<ResponseModel>
{
    public LoginRequestModel LoginModel { get; set; }
}
