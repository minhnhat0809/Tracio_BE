using MediatR;
using NotificationService.Application.Interfaces;
using Shared.Dtos;

namespace NotificationService.Application.Queries.Handlers;

public class GetNotificationByUserQueryHandler(INotificationRepo notificationRepo) : IRequestHandler<GetNotificationByUserQuery, ResponseDto>
{
    public async Task<ResponseDto> Handle(GetNotificationByUserQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return ResponseDto.GetSuccess(null, "Notification retrieved successfully");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }
}