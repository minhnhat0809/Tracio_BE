using System.Text.Json.Serialization;
using ContentService.Application.DTOs.BlogDtos;
using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Commands;

public class UpdateBlogCommand(int blogId, BlogUpdateDto blogUpdateDto) : IRequest<ResponseDto>
{
    public int BlogId { get; set; } = blogId;
    
    public string? Content {get; set;} = blogUpdateDto.Content;
    
    public sbyte? PrivacySetting { get; set; } = blogUpdateDto.PrivacySetting;
}