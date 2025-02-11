﻿using ContentService.Application.DTOs.CommentDtos;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.Dtos;

namespace ContentService.Application.Commands;

public class CreateCommentCommand(CommentCreateDto commentCreateDto, List<IFormFile> files) : IRequest<ResponseDto>
{
    public int CreatorId { get; set; } = commentCreateDto.CreatorId;
    
    public string CreatorName { get; set; } = commentCreateDto.CreatorName;
    
    public int BlogId { get; set; } = commentCreateDto.BlogId;
    
    public string Content { get; set; } = commentCreateDto.Content;

    public List<IFormFile> Files { get; set; } = files;
}