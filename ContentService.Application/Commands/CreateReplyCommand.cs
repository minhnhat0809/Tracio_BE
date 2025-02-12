using ContentService.Application.DTOs.ReplyDtos;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.Dtos;

namespace ContentService.Application.Commands;

public class CreateReplyCommand(ReplyCreateDto replyCreateDto, List<IFormFile> files) : IRequest<ResponseDto>
{
    public int CreatorId { get; set; } = replyCreateDto.CreatorId;
    
    public int CommentId { get; set; } = replyCreateDto.CommentId;
    
    public string Content { get; set; } = replyCreateDto.Content;

    public List<IFormFile>? MediaFiles { get; set; } = files;
}