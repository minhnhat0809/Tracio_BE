using ContentService.Application.DTOs.ReactionDtos.ViewDtos;
using ContentService.Application.Interfaces;
using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Queries.Handlers;

public class GetReactionByReplyQueryHandler(
    IReplyRepo replyRepo, 
    IReactionRepo reactionRepo, 
    ICacheService cacheService) 
    : IRequestHandler<GetReactionByReplyQuery, ResponseDto>
{
    private readonly IReplyRepo _replyRepo = replyRepo;
    private readonly IReactionRepo _reactionRepo = reactionRepo;
    private readonly ICacheService _cacheService = cacheService;

    public async Task<ResponseDto> Handle(GetReactionByReplyQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.ReplyId <= 0) return ResponseDto.BadRequest("ReplyId is required");
            
            var reply = await _replyRepo.GetByIdAsync(r => r.ReplyId == request.ReplyId, r => new{r.ReplyId, r.LikesCount});
            if (reply == null) return ResponseDto.NotFound("Reply not found");

            // **Cache Key**
            var cacheKey = $"Reactions:Reply{request.ReplyId}:Page{request.PageNumber}:Size{request.PageSize}";
            
            var totalReactions = reply.LikesCount;
            var totalPages = (int)Math.Ceiling((double)totalReactions / request.PageSize);

            // **Check if reactions are cached**
            var cachedReactions = await _cacheService.GetAsync<List<ReactionDto>>(cacheKey);
            if (cachedReactions != null)
            {
                return ResponseDto.GetSuccess(new
                {
                    reactions = cachedReactions,
                    request.PageNumber,
                    request.PageSize,
                    totalReactions = (long)totalReactions,
                    totalPages,
                    hasNextPage = request.PageNumber < totalPages,
                    hasPreviousPage = request.PageNumber > 1
                }, "Reactions retrieved successfully (cached)!");
            }

            // **Fetch reactions from DB**
            var reactionDtos = await _reactionRepo.FindAsyncWithPagingAndSorting(
                r => r.ReplyId == request.ReplyId, 
                r => new ReactionDto
                {
                    CyclistId = r.CyclistId,
                    CyclistName = r.CyclistName,
                    CyclistAvatar = r.CyclistAvatar
                },
                request.PageNumber, 
                request.PageSize
            );

            // **Cache the reactions for 5 minutes**
            await _cacheService.SetAsync(cacheKey, reactionDtos, TimeSpan.FromMinutes(5));

            return ResponseDto.GetSuccess(new
            {
                reactions = reactionDtos,
                request.PageNumber,
                request.PageSize,
                totalReactions = (long)totalReactions,
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
