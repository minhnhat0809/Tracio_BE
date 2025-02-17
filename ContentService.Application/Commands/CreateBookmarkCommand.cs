using ContentService.Application.DTOs.BookmarkDtos;
using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Commands;

public class CreateBookmarkCommand (int OwnerId, BookmarkCreateDto bookmarkCreateDto) : IRequest<ResponseDto>
{
    public int OwnerId { get; set; } = OwnerId;

    public int BlogId { get; set; } = bookmarkCreateDto.BlogId;

    public string? CollectionName { get; set; } = bookmarkCreateDto.CollectionName;
}