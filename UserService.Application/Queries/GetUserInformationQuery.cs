using MediatR;
using UserService.Application.DTOs.Users.View;

namespace UserService.Application.Queries;

public class GetUserInformationQuery(int userId) : IRequest<UserForBlogDto?>
{
    public int UserId { get; set; } = userId;
}