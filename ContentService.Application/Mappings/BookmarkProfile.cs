using AutoMapper;
using ContentService.Application.DTOs.BookmarkDtos;
using ContentService.Domain.Entities;

namespace ContentService.Application.Mappings;

public class BookmarkProfile : Profile
{
    public BookmarkProfile()
    {
        CreateMap<BookmarkCreateDto, BlogBookmark>();
    }
}