using MediatR;
using NotificationService.Application.Dtos.NotificationDtos.ViewDtos;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using Shared.Dtos;
using Shared.Ultilities;

namespace NotificationService.Application.Queries.Handlers;

public class GetNotificationByUserQueryHandler(INotificationRepo notificationRepo, IUserService userService) : IRequestHandler<GetNotificationByUserQuery, ResponseDto>
{
    private readonly INotificationRepo _notificationRepo = notificationRepo;
    
    private readonly IUserService _userService = userService;
    public async Task<ResponseDto> Handle(GetNotificationByUserQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var isUserExisted = await _userService.CheckUserValid(request.UserId);
            if(!isUserExisted) return ResponseDto.NotFound("User not found");

            var sortExpression = SortHelper.BuildSortExpression<Notification>("CreatedAt");

            var notificationDtos = await _notificationRepo.FindAsyncWithPagingAndSorting(n => n.RecipientId == request.UserId && n.IsDeleted != true, 
                n => new NotificationDto
                {
                    NotificationId = n.NotificationId,
                    CreatedAt = n.CreatedAt,
                    EntityId = n.EntityId,
                    EntityType = n.EntityType,
                    IsRead = n.IsRead,
                    Message = n.Message,
                    SenderName = n.SenderName,
                    SenderAvatar = n.SenderAvatar
                }, 
                request.PageNumber, request.PageSize, 
                sortExpression, false);
            
            var total = await _notificationRepo.CountAsync(n => n.RecipientId == request.UserId && n.IsDeleted != true);
            
            var totalPages = (int)Math.Ceiling((double)total / request.PageSize);
            
            return ResponseDto.GetSuccess(new
            {
                notifications = notificationDtos,
                totalNotifications = total,
                totalPages,
                hasNextPage = request.PageNumber < totalPages,
                hasPreviousPage = request.PageNumber > 1,
            }, "Notification retrieved successfully");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }
}