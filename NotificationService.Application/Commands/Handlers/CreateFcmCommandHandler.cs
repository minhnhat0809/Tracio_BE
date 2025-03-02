using AutoMapper;
using MediatR;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using Shared.Dtos;

namespace NotificationService.Application.Commands.Handlers;

public class CreateFcmCommandHandler(IDeviceFcmRepo deviceFcmRepo, IMapper mapper) : IRequestHandler<CreateFcmCommand, ResponseDto>
{
    private readonly IDeviceFcmRepo _deviceFcmRepo = deviceFcmRepo;
    
    private readonly IMapper _mapper = mapper;
    
    public async Task<ResponseDto> Handle(CreateFcmCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var isDeviceExisted = await _deviceFcmRepo.
                ExistsAsync(f => f.DeviceId == request.DeviceId && f.UserId == request.UserId);

            bool isSucceed;
            
            if (isDeviceExisted)
            {
                isSucceed = await _deviceFcmRepo.UpdateFieldsAsync(
                    f => f.DeviceId == request.DeviceId && f.UserId == request.UserId,
                    (f => f.FcmToken, request.FcmToken));
            }
            else
            {
                var deviceFcm = _mapper.Map<DeviceFcm>(request);
                
                isSucceed = await _deviceFcmRepo.CreateAsync(deviceFcm);
            }

            return isSucceed ? ResponseDto.CreateSuccess(request, "Create device successfully!") :
                ResponseDto.InternalError("Failed to create device");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }
}