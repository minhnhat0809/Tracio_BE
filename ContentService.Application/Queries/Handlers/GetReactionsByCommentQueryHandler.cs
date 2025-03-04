using ContentService.Application.DTOs.ReactionDtos.ViewDtos;
using ContentService.Application.Interfaces;
using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Queries.Handlers;

public class GetReactionsByCommentQueryHandler(
    IReactionRepo reactionRepo, 
    ICommentRepo commentRepo, 
    ICacheService cacheService) 
    : IRequestHandler<GetReactionsByCommentQuery, ResponseDto>
{
    private readonly IReactionRepo _reactionRepo = reactionRepo;
    private readonly ICommentRepo _commentRepo = commentRepo;
    private readonly ICacheService _cacheService = cacheService;

    public async Task<ResponseDto> Handle(GetReactionsByCommentQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.CommentId <= 0) return ResponseDto.BadRequest("Comment Id is required");

            // **Check if the comment exists**
            var comment = await _commentRepo.GetByIdAsync(c => c.CommentId == request.CommentId, c => new {c.CommentId, c.LikesCount});
            if (comment == null) return ResponseDto.NotFound("Comment not found");

            // **Cache Key**
            var reactionsCacheKey = $"Reactions:Comment{request.CommentId}:Page{request.PageNumber}:Size{request.PageSize}";

            // **Check if reactions are cached**
            var cachedReactions = await _cacheService.GetAsync<List<ReactionDto>>(reactionsCacheKey);
            if (cachedReactions != null)
            {
                var totalReactions = await _reactionRepo.CountAsync(r => r.CommentId == request.CommentId);
                var totalPages = (int)Math.Ceiling((double)totalReactions / request.PageSize);

                return ResponseDto.GetSuccess(new
                {
                    reactions = cachedReactions,
                    totalReactions,
                    totalPages,
                    hasNextPage = request.PageNumber < totalPages,
                    hasPreviousPage = request.PageNumber > 1
                }, "Reactions retrieved successfully (cached)!");
            }

            // **Fetch total reactions from DB (NOT CACHED)**
            var totalReactionsFromDb = comment.LikesCount;
            var totalPagesFromDb = (int)Math.Ceiling((double)totalReactionsFromDb / request.PageSize);

            // **Fetch reactions from DB**
            var reactionDtos = await _reactionRepo.FindAsyncWithPagingAndSorting(
                r => r.CommentId == request.CommentId,
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
            await _cacheService.SetAsync(reactionsCacheKey, reactionDtos, TimeSpan.FromMinutes(5));

            return ResponseDto.GetSuccess(new
            {
                reactions = reactionDtos,
                totalReactions = (long)totalReactionsFromDb,
                totalPages = totalPagesFromDb,
                hasNextPage = request.PageNumber < totalPagesFromDb,
                hasPreviousPage = request.PageNumber > 1
            }, "Reactions retrieved successfully!");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }
}
