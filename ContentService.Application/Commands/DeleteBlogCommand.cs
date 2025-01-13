using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Commands;

public record DeleteBlogCommand(int BlogId) : IRequest<ResponseDto>;