using ContentService.Application.DTOs.ReactionDtos.ViewDtos;
using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using ContentService.Domain.Enum;
using LinqKit;
using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Queries.Handlers;

public class GetReactionsByBlogIdHandler(IReactionRepo reactionRepo, IBlogRepo blogRepo) : IRequestHandler<GetReactionsByBlogIdQuery, ResponseDto>
{
    private readonly IReactionRepo _reactionRepo = reactionRepo;
    
    private readonly IBlogRepo _blogRepo = blogRepo;
    
    public async Task<ResponseDto> Handle(GetReactionsByBlogIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.BlogId)) return ResponseDto.BadRequest("Blog Id is required");
            
            // check blog in db
            var isBlogExisted = await _blogRepo.ExistsAsync(b => b.BlogId.Equals(request.BlogId));

            if (!isBlogExisted) return ResponseDto.NotFound($"Blog not found with this id : {request.BlogId}");

            // check if reaction type is valid
            if (!Enum.IsDefined(typeof(ReactionType), request.ReactionType))
            {
                return ResponseDto.BadRequest($"Invalid reaction type : {request.ReactionType}");
            }
            
            var basePredicate = PredicateBuilder.New<Reaction>(true);
            
            // build filter expression
            basePredicate = basePredicate
                .And(c => c.EntityType == EntityType.Blog &&
                          c.EntityId.Equals(request.BlogId) &&
                          c.EntityType == (EntityType)request.ReactionType);
            
            // count reactions
            var total = await _reactionRepo.CountAsync(basePredicate);
            if (total == 0) return ResponseDto.GetSuccess(
                new
                {
                    reactions = new List<ReactionDto>(),
                    total
                },
                "Not found reactions"
                );
            
            // fetch reactions
            var reactionsDto = await _reactionRepo.FindAsync(basePredicate,
                c => new ReactionDto
                {
                    UserId = c.UserId,
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