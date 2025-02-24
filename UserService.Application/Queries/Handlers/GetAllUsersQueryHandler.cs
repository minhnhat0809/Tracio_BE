using UserService.Application.Interfaces;

namespace UserService.Application.Queries.Handlers;

using MediatR;
using AutoMapper;
using DTOs.ResponseModel;
using DTOs.Users;
using UserService.Domain.Entities;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, ResponseModel>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetAllUsersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ResponseModel> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _unitOfWork.UserRepository.GetAllAsync(null, request.PageNumber, request.RowsPerPage, request.SortField, request.SortDesc);

        if (!string.IsNullOrEmpty(request.FilterField) && !string.IsNullOrEmpty(request.FilterValue))
        {
            users = ApplyFilter(users, request.FilterField, request.FilterValue);
        }

        var userViewModel = _mapper.Map<IEnumerable<UserViewModel>>(users);
        return new ResponseModel("success", 200, "Users retrieved successfully", new
        {
            TotalCount = users.Count(),
            PageNumber = request.PageNumber,
            RowsPerPage = request.RowsPerPage,
            Users = userViewModel
        });
    }

    private IEnumerable<User> ApplyFilter(IEnumerable<User> users, string field, string value)
    {
        return field switch
        {
            "UserName" => users.Where(u => u.UserName.Contains(value, StringComparison.OrdinalIgnoreCase)),
            "Email" => users.Where(u => u.Email.Contains(value, StringComparison.OrdinalIgnoreCase)),
            "PhoneNumber" => users.Where(u => u.PhoneNumber != null && u.PhoneNumber.Contains(value)),
            "City" => users.Where(u => u.City != null && u.City.Contains(value, StringComparison.OrdinalIgnoreCase)),
            _ => users
        };
    }
}
