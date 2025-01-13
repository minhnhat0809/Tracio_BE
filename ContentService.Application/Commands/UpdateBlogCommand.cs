using System.Text.Json.Serialization;
using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Commands;

public class UpdateBlogCommand : IRequest<ResponseDto>
{
    [JsonIgnore]
    public int BlogId {get; set;}
}