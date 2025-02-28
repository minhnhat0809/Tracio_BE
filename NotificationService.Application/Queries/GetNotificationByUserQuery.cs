using MediatR;
using Shared.Dtos;

namespace NotificationService.Application.Queries;

public class GetNotificationByUserQuery(int userId, int pageSize, int pageNumber) : IRequest<ResponseDto>
{
    public int UserId { get; set; } = userId;
    
    public int PageNumber { get; set; } = pageNumber;
    
    public int PageSize { get; set; } = pageSize;
}