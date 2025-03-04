using AutoMapper;
using NotificationService.Application.Commands;
using NotificationService.Domain.Entities;

namespace NotificationService.Application.Mappings;

public class DeviceFcmProfile : Profile
{
    public DeviceFcmProfile()
    {
        CreateMap<CreateFcmCommand, DeviceFcm>();
    }
}