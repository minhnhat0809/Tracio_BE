using AutoMapper;
using MediatR;
using UserService.Application.DTOs.ResponseModel;
using UserService.Application.DTOs.Users;
using UserService.Application.Interfaces;

namespace UserService.Application.Commands.Handlers;

public class UnBanUserCommandHandler: IRequestHandler<UnBanUserCommand, ResponseModel>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UnBanUserCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ResponseModel> Handle(UnBanUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(request.UserId, "");
        if (user == null)
            return new ResponseModel("error", 404, "User not found", null);

        // UnBan User == Active Account
        user.IsActive = true;
        await _unitOfWork.UserRepository.UpdateAsync(user);

        return new ResponseModel("success", 200, "Unban user successfully", _mapper.Map<UserViewModel>(user));
    }
}