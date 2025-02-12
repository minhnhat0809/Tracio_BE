using AutoMapper;
using ContentService.Application.DTOs.ReplyDtos.View;
using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using MediatR;
using Shared.Dtos;
using Shared.Ultilities;

namespace ContentService.Application.Queries.Handlers;

public class GetRepliesByCommentQueryHandler(IMapper mapper, ICommentRepo commentRepo, IReplyRepo replyRepo) : IRequestHandler<GetRepliesByCommentQuery, ResponseDto>
{
    private readonly ICommentRepo _commentRepo = commentRepo;
    
    private readonly IReplyRepo _replyRepo = replyRepo;

    private readonly IMapper _mapper = mapper;
    
    public async Task<ResponseDto> Handle(GetRepliesByCommentQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // check comment in db
            var isCommentExisted = await _commentRepo.ExistsAsync(c => c.CommentId == request.CommentId);
            if (!isCommentExisted) return ResponseDto.NotFound("Comment not found!");
            
            // build sort expression
            var sortExpression = SortHelper.BuildSortExpression<Reply>("CreatedAt");

            var total = await _replyRepo.CountAsync(r => r.CommentId == request.CommentId && r.IsDeleted == false);

            var replyDtos = await _replyRepo.FindAsyncWithPagingAndSorting(r => r.CommentId == request.CommentId && r.IsDeleted == false,
                r => 
                new ReplyDto
                {
                    ReplyId = r.ReplyId,
                    CommentId = r.CommentId,
                    CyclistId = r.CyclistId,
                    CyclistName = r.CyclistName,
                    Content = r.Content,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    LikesCount = r.LikesCount,
                    IsEdited = r.IsEdited
                }, request.PageNumber, request.PageSize,
                sortExpression, false);
            
            
            return ResponseDto.GetSuccess(new
            {
                replies = replyDtos,
                total
            }, "Replies retrieved successfully.");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }
}