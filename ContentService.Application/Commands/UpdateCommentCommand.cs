using System.Text.Json.Serialization;
using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Commands;

public class UpdateCommentCommand : IRequest<ResponseDto>
{
    [JsonIgnore]
    public int CommentId { get; set; }
}