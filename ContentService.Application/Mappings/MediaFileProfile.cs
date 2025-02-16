using AutoMapper;
using ContentService.Application.DTOs.MediaFileDTOs.ViewDtos;
using ContentService.Domain.Entities;

namespace ContentService.Application.Mappings;

public class MediaFileProfile : Profile
{
    public MediaFileProfile()
    {
        CreateMap<MediaFile, MediaFileDto>();
    }
}