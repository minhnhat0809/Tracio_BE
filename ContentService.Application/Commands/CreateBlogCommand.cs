using ContentService.Application.DTOs.BlogDtos;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.Dtos;

namespace ContentService.Application.Commands;

public class CreateBlogCommand(BlogCreateDto blogCreateDto) : IRequest<ResponseDto>
{
    public int CreatorId { get; set; } = blogCreateDto.CreatorId;

    public int CategoryId { get; set; } = blogCreateDto.CategoryId;

    public IFormFileCollection? MediaFiles { get; set; } = blogCreateDto.MediaFiles;

    public string Content { get; set; } = blogCreateDto.Content;

    public string PrivacySetting { get; set; } = blogCreateDto.PrivacySetting;

    public sbyte Status { get; set; } = blogCreateDto.Status;
}