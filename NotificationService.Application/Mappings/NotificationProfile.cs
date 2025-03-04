using AutoMapper;
using NotificationService.Application.Dtos.NotificationDtos.ViewDtos;
using NotificationService.Domain.Entities;
using NotificationService.Domain.Enums;
using Shared.Dtos.Messages;

namespace NotificationService.Application.Mappings;

public class NotificationProfile : Profile
{
    public NotificationProfile()
    {
        CreateMap<NotificationEvent, Notification>()
            .ForMember(dest => dest.EntityType, opt => opt.MapFrom(src => ConvertEntityType(src.EntityType)));

        
        CreateMap<Notification, NotificationDto>();
    }

    private static sbyte ConvertEntityType(string entityType)
    {
        if (Enum.TryParse(typeof(EntityType), entityType, out var result))
        {
            return (sbyte)(EntityType)result!;
        }
        throw new ArgumentException($"Invalid entity type: {entityType}");
    }
}