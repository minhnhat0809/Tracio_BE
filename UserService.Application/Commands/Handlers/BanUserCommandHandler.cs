using AutoMapper;
using MediatR;
using UserService.Application.DTOs.ResponseModel;
using UserService.Application.DTOs.Users;
using UserService.Application.Interfaces;

namespace UserService.Application.Commands.Handlers;

public class BanUserCommandHandler: IRequestHandler<BanUserCommand, ResponseModel>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public BanUserCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ResponseModel> Handle(BanUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(request.UserId,"");
        if (user == null)
            return new ResponseModel("error", 404, "User not found", null);

        // Ban User == UnActive Account
        user.IsActive = false;
        await _unitOfWork.UserRepository.UpdateAsync(user);

        return new ResponseModel("success", 200, "Ban user successfully", _mapper.Map<UserViewModel>(user));
    }
}