using ContentService.Application.DTOs.ReactionDtos.ViewDtos;
using ContentService.Application.Interfaces;
using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Queries.Handlers;

public class GetReactionsByBlogQueryHandler(
    IReactionRepo reactionRepo, 
    IBlogRepo blogRepo, 
    ICacheService cacheService) 
    : IRequestHandler<GetReactionsByBlogQuery, ResponseDto>
{
    private readonly IReactionRepo _reactionRepo = reactionRepo;
    private readonly IBlogRepo _blogRepo = blogRepo;
    private readonly ICacheService _cacheService = cacheService;

    public async Task<ResponseDto> Handle(GetReactionsByBlogQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.BlogId <= 0) return ResponseDto.BadRequest("Blog Id is required");

            // Check if the blog exists
            var blog = await _blogRepo.GetByIdAsync(b => b.BlogId == request.BlogId, b => new{b.BlogId, b.ReactionsCount});
            if (blog == null) return ResponseDto.NotFound($"Blog not found with this id: {request.BlogId}");

            // **Cache keys**
            var reactionsCacheKey = $"Reactions:Blog{request.BlogId}:Page{request.PageNumber}:Size{request.PageSize}";
            
            var totalReactions = blog.ReactionsCount;

            var totalPages = (int)Math.Ceiling((double)totalReactions / request.PageSize);

            // **Check if reactions are cached**
            var cachedReactions = await _cacheService.GetAsync<List<ReactionDto>>(reactionsCacheKey);
            if (cachedReactions != null)
            {
                return ResponseDto.GetSuccess(new
                {
                    reactions = cachedReactions,
                    totalReactions,
                    totalPages,
                    hasNextPage = request.PageNumber < totalPages,
                    hasPreviousPage = request.PageNumber > 1
                }, "Reactions retrieved successfully (cached)!");
            }

            // **Fetch reactions from DB if not cached**
            var reactionsDto = await _reactionRepo.FindAsyncWithPagingAndSorting(
                r => r.BlogId == request.BlogId,
                r => new ReactionDto
                {
                    CyclistId = r.CyclistId,
                    CyclistName = r.CyclistName,
                    CyclistAvatar = r.CyclistAvatar
                },
                request.PageNumber,
                request.PageSize
            );

            // **Cache reactions for 5 minutes**
            await _cacheService.SetAsync(reactionsCacheKey, reactionsDto, TimeSpan.FromMinutes(5));

            return ResponseDto.GetSuccess(new
            {
                reactions = reactionsDto,
                totalReactions,
                totalPages,
                hasNextPage = request.PageNumber < totalPages,
                hasPreviousPage = request.PageNumber > 1
            }, "Reactions retrieved successfully!");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }
}
