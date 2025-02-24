using AutoMapper;
using MediatR;
using UserService.Application.DTOs.ResponseModel;
using UserService.Application.DTOs.Users;
using UserService.Application.Interfaces;

namespace UserService.Application.Queries.Handlers;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, ResponseModel>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetUserByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ResponseModel> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.UserRepository.GetByIdAsync(request.UserId,"");
        if (user == null)
            return new ResponseModel("error", 404, "User not found", null);

        var userModel = _mapper.Map<UserViewModel>(user);
        return new ResponseModel("success", 200, "User retrieved successfully", userModel);
    }
}