using MediatR;
using Shared.Dtos;

namespace NotificationService.Application.Queries;

public class GetNotificationByUserQuery(int userId) : IRequest<ResponseDto>
{
    public int UserId { get; set; } = userId;
}