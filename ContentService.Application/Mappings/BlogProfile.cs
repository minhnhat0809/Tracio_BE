using AutoMapper;
using ContentService.Application.DTOs.BlogDtos;
using ContentService.Application.DTOs.BlogDtos.ViewDtos;
using ContentService.Domain.Entities;

namespace ContentService.Application.Mappings;

public class BlogProfile : Profile
{
    public BlogProfile()
    {
        CreateMap<Blog, BlogWithCommentsDto>();
    }
}