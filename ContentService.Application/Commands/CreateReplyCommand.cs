using ContentService.Application.DTOs.ReplyDtos;
using MediatR;
using Microsoft.AspNetCore.Http;
using Shared.Dtos;

namespace ContentService.Application.Commands;

public class CreateReplyCommand(int creatorId, ReplyCreateDto replyCreateDto, List<IFormFile> files) : IRequest<ResponseDto>
{
    public int CyclistId { get; set; } = creatorId;
    
    public int CommentId { get; set; } = replyCreateDto.CommentId;
    
    public string Content { get; set; } = replyCreateDto.Content;

    public List<IFormFile>? MediaFiles { get; set; } = files;
}