using AutoMapper;
using ContentService.Application.DTOs.ReactionDtos.ViewDtos;
using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using MediatR;
using Shared.Dtos;
using ContentService.Domain.Enums;

namespace ContentService.Application.Commands.Handlers;

public class CreateReactionCommandHandler(
    IReactionRepo reactionRepo, 
    IBlogRepo blogRepo,
    ICommentRepo commentRepo, 
    IReplyRepo replyRepo,
    IMapper mapper) 
    : IRequestHandler<CreateReactionCommand, ResponseDto>
{
    public async Task<ResponseDto> Handle(CreateReactionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate reaction type using an enum
            if (!Enum.TryParse(typeof(ReactionType), request.ReactionType, out var _))
            {
                return ResponseDto.BadRequest("Invalid reaction type. Allowed values: Like, Dislike, Love, Angry, Wow.");
            }

            return request.EntityType.ToLower() switch
            {
                "reply" => await HandleReplyReaction(request),
                "blog" => await HandleBlogReaction(request),
                "comment" => await HandleCommentReaction(request),
                _ => ResponseDto.BadRequest("Invalid entity type. Allowed values: reply, blog, comment.")
            };
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }

    private async Task<ResponseDto> HandleReplyReaction(CreateReactionCommand request)
    {
        var reply = await replyRepo.GetByIdAsync(r => r.ReplyId == request.EntityId, r => r);
        if (reply == null) return ResponseDto.NotFound("Reply not found");

        var reaction = new Reaction
        {
            CyclistId = request.CyclistId,
            CyclistName = request.CyclistName,
            ReplyId = request.EntityId,
            ReactionType = request.ReactionType,
            CreatedAt = DateTime.UtcNow
        };

        var isSucceed = await reactionRepo.CreateAsync(reaction);
        if (!isSucceed) return ResponseDto.InternalError("Failed to create reaction.");

        reply.LikesCount++;
        await replyRepo.UpdateAsync(reply.ReplyId, reply);
        
        var reactionDto = mapper.Map<ReactionDto>(reaction);

        return ResponseDto.CreateSuccess(reactionDto, "Reaction created successfully!");
    }

    private async Task<ResponseDto> HandleBlogReaction(CreateReactionCommand request)
    {
        var blog = await blogRepo.GetByIdAsync(b => b.BlogId == request.EntityId, b => b);
        if (blog == null) return ResponseDto.NotFound("Blog not found");

        var reaction = new Reaction
        {
            CyclistId = request.CyclistId,
            CyclistName = request.CyclistName,
            BlogId = request.EntityId,
            ReactionType = request.ReactionType,
            CreatedAt = DateTime.UtcNow
        };

        var isSucceed = await reactionRepo.CreateAsync(reaction);
        if (!isSucceed) return ResponseDto.InternalError("Failed to create reaction.");

        blog.ReactionsCount++;
        await blogRepo.UpdateAsync(blog.BlogId, blog);
        
        var reactionDto = mapper.Map<ReactionDto>(reaction);

        return ResponseDto.CreateSuccess(reactionDto, "Reaction created successfully!");
    }

    private async Task<ResponseDto> HandleCommentReaction(CreateReactionCommand request)
    {
        var comment = await commentRepo.GetByIdAsync(c => c.CommentId == request.EntityId, c => c);
        if (comment == null) return ResponseDto.NotFound("Comment not found");

        var reaction = new Reaction
        {
            CyclistId = request.CyclistId,
            CyclistName = request.CyclistName,
            ReplyId = request.EntityId,
            ReactionType = request.ReactionType,
            CreatedAt = DateTime.UtcNow
        };

        var isSucceed = await reactionRepo.CreateAsync(reaction);
        if (!isSucceed) return ResponseDto.InternalError("Failed to create reaction.");

        comment.LikesCount++;
        await commentRepo .UpdateAsync(comment.CommentId, comment);
        
        var reactionDto = mapper.Map<ReactionDto>(reaction);

        return ResponseDto.CreateSuccess(reactionDto, "Reaction created successfully!");
    }
}
