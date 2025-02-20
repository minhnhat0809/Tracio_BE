namespace UserService.Application.Commands;

using MediatR;
using UserService.Application.DTOs.Users;
using UserService.Application.DTOs.ResponseModel;

public class ShopRegisterCommand : IRequest<ResponseModel>
{
    public ShopOwnerRegisterModel? RegisterModel { get; set; }
}
