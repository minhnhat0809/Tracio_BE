using ContentService.Application.DTOs.ReactionDtos.ViewDtos;
using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using LinqKit;
using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Queries.Handlers;

public class GetReactionsByBlogQueryHandler(IReactionRepo reactionRepo, IBlogRepo blogRepo) : IRequestHandler<GetReactionsByBlogQuery, ResponseDto>
{
    private readonly IReactionRepo _reactionRepo = reactionRepo;
    
    private readonly IBlogRepo _blogRepo = blogRepo;
    
    public async Task<ResponseDto> Handle(GetReactionsByBlogQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.BlogId <= 0) return ResponseDto.BadRequest("Blog Id is required");
            
            // check blog in db
            var isBlogExisted = await _blogRepo.ExistsAsync(b => b.BlogId.Equals(request.BlogId));

            if (!isBlogExisted) return ResponseDto.NotFound($"Blog not found with this id : {request.BlogId}");
            
            var basePredicate = PredicateBuilder.New<Reaction>(true);
            
            basePredicate = basePredicate.And(r => r.BlogId == request.BlogId);
            
            // count reactions
            var total = await _reactionRepo.CountAsync(basePredicate);
            
            // fetch reactions
            var reactionsDto = await _reactionRepo.FindAsync(basePredicate,
                c => new ReactionDto
                {
                    CyclistId = c.CyclistId,
                    CyclistName = c.CyclistName,
                    CyclistAvatar = c.CyclistAvatar,
                    CreatedAt = c.CreatedAt
                });

            // sort by created at
            reactionsDto = reactionsDto.OrderByDescending(c => c.CreatedAt).ToList();

            return ResponseDto.GetSuccess(
                new
                {
                    reactions = reactionsDto,
                    total
                },
                "Reactions retrieved successfully!");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }
}