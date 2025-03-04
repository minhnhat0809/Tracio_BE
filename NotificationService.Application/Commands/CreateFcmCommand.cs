using MediatR;
using NotificationService.Application.Dtos.FcmDtos;
using Shared.Dtos;

namespace NotificationService.Application.Commands;

public class CreateFcmCommand(FcmCreateDto fcmCreateDto, int userId) : IRequest<ResponseDto>
{
    public int UserId { get; set; } = userId;
    
    public string DeviceId { get; set; } = fcmCreateDto.DeviceId;
    
    public string FcmToken { get; set; } = fcmCreateDto.FcmToken;
}