namespace UserService.Application.Commands;

using MediatR;
using UserService.Application.DTOs.ResponseModel;
using UserService.Application.DTOs.Users;

public record UpdateUserCommand
    : IRequest<ResponseModel>
{
    public int? UserId { get; set; }
    public UpdateUserProfileModel? UserModel { get; set; }
}
