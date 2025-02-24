using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Commands;

public class DeleteBookmarkCommand(int bookmarkId) : IRequest<ResponseDto>
{
    public int BookmarkId { get; set; } = bookmarkId;
}