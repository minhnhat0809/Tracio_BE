using MediatR;
using UserService.Application.DTOs.Auths;
using UserService.Application.DTOs.ResponseModel;

namespace UserService.Application.Commands;

public class RefreshTokenCommand: IRequest<ResponseModel>
{
    public RefreshTokenRequestModel? RegisterModel { get; set; }
}
