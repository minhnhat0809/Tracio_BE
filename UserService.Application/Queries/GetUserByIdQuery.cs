using MediatR;
using UserService.Application.DTOs.ResponseModel;

namespace UserService.Application.Queries;

public class GetUserByIdQuery : IRequest<ResponseModel>
{
    public int UserId { get; set; }
}
