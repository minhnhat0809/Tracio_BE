using UserService.Application.Interfaces;

namespace UserService.Application.Commands.Handlers;
using MediatR;
using UserService.Application.DTOs.ResponseModel;

public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, ResponseModel>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteUserCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ResponseModel> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(request.UserId);
        if (user == null)
            return new ResponseModel("error", 404, "User not found", null);

        user.IsActive = false;
        await _unitOfWork.UserRepository.UpdateAsync(user);
        return new ResponseModel("success", 200, "User deleted successfully", null);
    }
}
