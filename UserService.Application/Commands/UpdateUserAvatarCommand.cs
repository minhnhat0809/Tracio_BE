using MediatR;
using Microsoft.AspNetCore.Http;
using UserService.Application.DTOs.ResponseModel;

namespace UserService.Application.Commands;

public class UpdateUserAvatarCommand
    : IRequest<ResponseModel>
{
    public int? UserId { get; set; }
    public IFormFile? Avatar { get; set; }
}

