using UserService.Application.Interfaces;

namespace UserService.Application.Commands.Handlers;

using MediatR;
using AutoMapper;
using UserService.Application.DTOs.ResponseModel;
using UserService.Application.DTOs.Users;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, ResponseModel>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateUserCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ResponseModel> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(request.UserId);
        if (user == null)
            return new ResponseModel("error", 404, "User not found", null);

        _mapper.Map(request.UserModel, user);
        await _unitOfWork.UserRepository.UpdateAsync(user);

        return new ResponseModel("success", 200, "User updated successfully", _mapper.Map<UserViewModel>(user));
    }
}
