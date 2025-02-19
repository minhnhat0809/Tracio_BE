using ContentService.Application.DTOs.BlogDtos;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.Dtos;

namespace ContentService.Application.Commands;

public class CreateBlogCommand(int CreatorId, BlogCreateDto blogCreateDto, List<IFormFile> mediaFiles) : IRequest<ResponseDto>
{
    public int CreatorId { get; set; } = CreatorId;

    public int CategoryId { get; set; } = blogCreateDto.CategoryId;

    public List<IFormFile>? MediaFiles { get; set; } = mediaFiles;

    public string Content { get; set; } = blogCreateDto.Content;

    public sbyte PrivacySetting { get; set; } = blogCreateDto.PrivacySetting;

    public sbyte Status { get; set; } = blogCreateDto.Status;
}