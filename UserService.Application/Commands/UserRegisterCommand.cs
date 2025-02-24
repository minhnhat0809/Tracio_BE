namespace UserService.Application.Commands;
using MediatR;
using UserService.Application.DTOs.Users;
using UserService.Application.DTOs.ResponseModel;

public class UserRegisterCommand : IRequest<ResponseModel>
{
    public UserRegisterModel? RegisterModel { get; set; }
}
