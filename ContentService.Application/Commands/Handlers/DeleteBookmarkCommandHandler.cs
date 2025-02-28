using ContentService.Application.Interfaces;
using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Commands.Handlers;

public class DeleteBookmarkCommandHandler(IBookmarkRepo bookmarkRepo) : IRequestHandler<DeleteBookmarkCommand, ResponseDto>
{
    private readonly IBookmarkRepo _bookmarkRepo = bookmarkRepo;
    
    public async Task<ResponseDto> Handle(DeleteBookmarkCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var bookmarkId = await _bookmarkRepo.GetByIdAsync(bm => bm.BookmarkId == request.BlogId && bm.OwnerId == request.UserRequestId, 
                bm => bm.BookmarkId);
            if(bookmarkId <= 0) return ResponseDto.NotFound("Bookmark not found");
            
            var result = await _bookmarkRepo.DeleteAsync(bookmarkId);
            
            return result ? ResponseDto.DeleteSuccess("Bookmark deleted successfully!"):
                    ResponseDto.InternalError("Failed to delete bookmark");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }
}