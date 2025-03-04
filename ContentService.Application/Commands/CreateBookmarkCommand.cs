using ContentService.Application.DTOs.BookmarkDtos;
using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Commands;

public class CreateBookmarkCommand (int ownerId, BookmarkCreateDto bookmarkCreateDto) : IRequest<ResponseDto>
{
    public int OwnerId { get; set; } = ownerId;

    public int BlogId { get; set; } = bookmarkCreateDto.BlogId;
}