using MediatR;
using UserService.Application.Interfaces;

namespace UserService.Application.Queries.Handlers;

public class CheckUserValidQueryHandler(IUserRepository userRepository) : IRequestHandler<CheckUserValidQuery, bool>
{
    private readonly IUserRepository _userRepository = userRepository;
    
    public async Task<bool> Handle(CheckUserValidQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var isUserExisted = await _userRepository.ExistsAsync(u => u.UserId == request.UserId);
            
            return isUserExisted;
        }
        catch (Exception e)
        {
            return false;
        }
    }
}