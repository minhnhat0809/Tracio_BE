using ContentService.Application.DTOs.CommentDtos.ViewDtos;
using ContentService.Application.DTOs.MediaFileDTOs.ViewDtos;
using ContentService.Application.DTOs.ReplyDtos.View;
using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using MediatR;
using Shared.Dtos;
using Shared.Ultilities;

namespace ContentService.Application.Queries.Handlers;

public class GetRepliesByCommentQueryHandler(ICommentRepo commentRepo, IReplyRepo replyRepo) : IRequestHandler<GetRepliesByCommentQuery, ResponseDto>
{
    private readonly ICommentRepo _commentRepo = commentRepo;
    
    private readonly IReplyRepo _replyRepo = replyRepo;

    public async Task<ResponseDto> Handle(GetRepliesByCommentQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var comment = new CommentDto();
            
            // handle the event when user click notification
            if (request.ReplyId.HasValue)
            {
                var replyAndCommentIndex = await _replyRepo.GetReplyIndex(request.ReplyId.Value);

                if (!replyAndCommentIndex.Equals((-1, -1)))
                {
                    request.PageNumber = (int)Math.Ceiling((double) replyAndCommentIndex.ReplyIndex / request.PageSize);
                    
                    request.CommentId = replyAndCommentIndex.CommentId;
                }
                
                comment = await _commentRepo.GetByIdAsync(c => c.CommentId == request.CommentId, c => new CommentDto
                {
                    CommentId = c.CommentId,
                    UserId = c.CyclistId,
                    UserName = c.CyclistName,
                    Avatar = c.CyclistAvatar,
                    Content = c.Content,
                    MediaFiles = c.MediaFiles.Select(mf => new MediaFileDto { MediaId = mf.MediaId, MediaUrl = mf.MediaUrl }).ToList(),
                    IsReacted = c.Reactions.Any(rr => rr.CyclistId == request.UserRequestId),
                    CreatedAt = c.CreatedAt,
                    LikesCount = c.LikesCount!.Value
                });
            }
            else
            {
                // check comment in db
                var isCommentExisted = await _commentRepo.ExistsAsync(c => c.CommentId == request.CommentId);
                if (!isCommentExisted) return ResponseDto.NotFound("Comment not found!");   
            }
            
            // build sort expression
            var sortExpression = SortHelper.BuildSortExpression<Reply>("CreatedAt");

            var totalReplies = await _replyRepo.CountAsync(r => r.CommentId == request.CommentId && r.IsDeleted == false);

            var totalPages = (int)Math.Ceiling((double)totalReplies / request.PageSize);

            var replyDtos = await _replyRepo.FindAsyncWithPagingAndSorting(r => r.CommentId == request.CommentId && r.IsDeleted != true,
                r => 
                new ReplyDto
                {
                    ReplyId = r.ReplyId,
                    CommentId = r.CommentId,
                    CyclistId = r.CyclistId,
                    CyclistName = r.CyclistName,
                    Content = r.Content,
                    MediaFiles = r.MediaFiles.Select(mf => new MediaFileDto { MediaId = mf.MediaId, MediaUrl = mf.MediaUrl }).ToList(),
                    IsReacted = r.Reactions.Any(rr => rr.CyclistId == request.UserRequestId),
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    LikesCount = r.LikesCount
                }, request.PageNumber, request.PageSize,
                sortExpression, false);
            
            
            return ResponseDto.GetSuccess(new
            {
                comment,
                replies = replyDtos,
                request.PageNumber,
                request.PageSize,
                totalReplies,
                totalPages,
                hasNextPage = request.PageNumber < totalPages,
                hasPreviousPage = request.PageNumber > 1
            }, "Replies retrieved successfully.");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }
}