using AutoMapper;
using ContentService.Application.Commands;
using ContentService.Application.DTOs.BlogDtos;
using ContentService.Application.DTOs.BlogDtos.ViewDtos;
using ContentService.Domain.Entities;

namespace ContentService.Application.Mappings;

public class BlogProfile : Profile
{
    public BlogProfile()
    {
        CreateMap<Blog, BlogWithCommentsDto>();

        CreateMap<CreateBlogCommand, Blog>()
            .ForMember(dest => dest.MediaFiles, opt => opt.Ignore());
    }
}