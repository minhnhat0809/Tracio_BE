using MediatR;
using NotificationService.Application.Interfaces;
using Shared.Dtos;

namespace NotificationService.Application.Commands.Handlers;

public class DeleteFcmCommandHandler(IDeviceFcmRepo deviceFcmRepo) : IRequestHandler<DeleteFcmCommand, ResponseDto>
{
    private readonly IDeviceFcmRepo _deviceFcmRepo = deviceFcmRepo;
    
    public async Task<ResponseDto> Handle(DeleteFcmCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var isFcmExisted = await _deviceFcmRepo.ExistsAsync(f => f.DeviceId == request.DeviceId);
            if (!isFcmExisted) return ResponseDto.NotFound("Device not found");
            
            await _deviceFcmRepo.DeleteAsync(request.DeviceId);
            
            return ResponseDto.DeleteSuccess("Device deleted");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }
}