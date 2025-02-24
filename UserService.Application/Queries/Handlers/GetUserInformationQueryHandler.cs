using MediatR;
using UserService.Application.DTOs.Users.View;
using UserService.Application.Interfaces;

namespace UserService.Application.Queries.Handlers;

public class GetUserInformationQueryHandler(IUserRepository userRepository) : IRequestHandler<GetUserInformationQuery, UserForBlogDto?>
{
    private readonly IUserRepository _userRepository = userRepository;
    
    public async Task<UserForBlogDto?> Handle(GetUserInformationQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetById(u => u.UserId == request.UserId, u => new UserForBlogDto(u.UserName, u.ProfilePicture));

            return user;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}