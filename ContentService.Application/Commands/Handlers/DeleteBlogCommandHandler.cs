using ContentService.Application.Interfaces;
using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Commands.Handlers;

public class DeleteBlogCommandHandler(IBlogRepo blogRepo) : IRequestHandler<DeleteBlogCommand, ResponseDto>
{
    private readonly IBlogRepo _blogRepo = blogRepo;
    
    public async Task<ResponseDto> Handle(DeleteBlogCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // fetch blog
            var blogIsExisted = await _blogRepo.ExistsAsync(b => b.BlogId == request.BlogId);
            if (!blogIsExisted) return ResponseDto.NotFound("Blog not found");
            
            //delete blog
            var isSucceed = await _blogRepo.ArchiveBlog(request.BlogId);
            
            return !isSucceed ? ResponseDto.InternalError("Failed to delete blog") : 
                ResponseDto.DeleteSuccess("Blog deleted successfully!");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }
}