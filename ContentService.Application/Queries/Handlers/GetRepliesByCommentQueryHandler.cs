using ContentService.Application.DTOs.CommentDtos.ViewDtos;
using ContentService.Application.DTOs.MediaFileDTOs.ViewDtos;
using ContentService.Application.DTOs.ReplyDtos.View;
using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using MediatR;
using Shared.Dtos;
using Shared.Ultilities;

namespace ContentService.Application.Queries.Handlers;

public class GetRepliesByCommentQueryHandler(
    ICommentRepo commentRepo, 
    IReplyRepo replyRepo, 
    ICacheService cacheService) 
    : IRequestHandler<GetRepliesByCommentQuery, ResponseDto>
{
    private readonly ICommentRepo _commentRepo = commentRepo;
    private readonly IReplyRepo _replyRepo = replyRepo;
    private readonly ICacheService _cacheService = cacheService;

    public async Task<ResponseDto> Handle(GetRepliesByCommentQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var comment = new CommentDto();
            
            // Cache keys for comment and replies
            var commentCacheKey = $"Comment:{request.CommentId}";
            var repliesCacheKey = $"Replies:Comment{request.CommentId}:Page{request.PageNumber}:Size{request.PageSize}";

            // Handle the event when user clicks notification
            if (request.ReplyId.HasValue)
            {
                var replyAndCommentIndex = await _replyRepo.GetReplyIndex(request.ReplyId.Value);

                if (replyAndCommentIndex.Equals((-1, -1, -1)))
                {
                    return ResponseDto.NotFound("Reply not found");
                }
                
                request.PageNumber = (int)Math.Ceiling((double) replyAndCommentIndex.ReplyIndex / request.PageSize);
                request.CommentId = replyAndCommentIndex.CommentId;
                
                repliesCacheKey = $"Replies:Comment{request.CommentId}:Page{request.PageNumber}:Size{request.PageSize}";

                // **Check if comment details are cached**
                comment = await _cacheService.GetAsync<CommentDto>(commentCacheKey);
                if (comment == null)
                {
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
                        LikesCount = c.LikesCount,
                        RepliesCount = c.RepliesCount
                    });

                    if (comment != null)
                    {
                        // Cache the comment for 10 minutes
                        await _cacheService.SetAsync(commentCacheKey, comment, TimeSpan.FromMinutes(10));
                    }
                }

                if (replyAndCommentIndex.ReReplyId > 0)
                {
                    var reReply = await _replyRepo.GetByIdAsync(r => r.ReplyId == replyAndCommentIndex.ReReplyId, 
                        r => new ReplyDto
                        {
                            ReplyId = r.ReplyId,
                            CommentId = r.CommentId,
                            CyclistId = r.CyclistId,
                            CyclistName = r.CyclistName,
                            Content = r.Content,
                            MediaFiles = r.MediaFiles.Select(mf => new MediaFileDto { MediaId = mf.MediaId, MediaUrl = mf.MediaUrl }).ToList(),
                            IsReacted = r.Reactions.Any(rr => rr.CyclistId == request.UserRequestId),
                            CreatedAt = r.CreatedAt,
                            LikesCount = r.LikesCount
                        });
                    
                    // **Check if replies are cached**
                    var cachedReplies1 = await _cacheService.GetAsync<List<ReplyDto>>(repliesCacheKey);
                    if (cachedReplies1 != null)
                    {
                        var totalRepliesFromCache = await _replyRepo.CountAsync(r => r.CommentId == request.CommentId && !r.IsDeleted);
                        var totalPagesFromCache = (int)Math.Ceiling((double)totalRepliesFromCache / request.PageSize);
                        
                        if (cachedReplies1.All(reply => reply.ReplyId != reReply!.ReplyId))
                        {
                            cachedReplies1.Insert(0, reReply!);
                        }

                        return ResponseDto.GetSuccess(new
                        {
                            comment,
                            replies = cachedReplies1.OrderBy(r => r.CreatedAt),
                            request.PageNumber,
                            request.PageSize,
                            totalReplies = totalRepliesFromCache,
                            totalPages = totalPagesFromCache,
                            hasNextPage = request.PageNumber < totalPagesFromCache,
                            hasPreviousPage = request.PageNumber > 1
                        }, "Replies retrieved successfully (cached).");
                    }

                    var totalReplies = await _replyRepo.CountAsync(r => r.CommentId == request.CommentId && !r.IsDeleted);
                    var totalPages = (int)Math.Ceiling((double)totalReplies / request.PageSize);
                    var sortExpression = SortHelper.BuildSortExpression<Reply>("CreatedAt");

                    var replyDtos = await _replyRepo.FindAsyncWithPagingAndSorting(
                        r => r.CommentId == request.CommentId && !r.IsDeleted,
                        r => new ReplyDto
                        {
                            ReplyId = r.ReplyId,
                            CommentId = r.CommentId,
                            CyclistId = r.CyclistId,
                            CyclistName = r.CyclistName,
                            Content = r.Content,
                            MediaFiles = r.MediaFiles.Select(mf => new MediaFileDto { MediaId = mf.MediaId, MediaUrl = mf.MediaUrl }).ToList(),
                            IsReacted = r.Reactions.Any(rr => rr.CyclistId == request.UserRequestId),
                            CreatedAt = r.CreatedAt,
                            LikesCount = r.LikesCount
                        },
                        request.PageNumber, 
                        request.PageSize, 
                        sortExpression, 
                        false
                    );
                    
                    // Cache replies for 5 minutes
                    await _cacheService.SetAsync(repliesCacheKey, replyDtos, TimeSpan.FromMinutes(5));

                    if (replyDtos.All(reply => reply.ReplyId != reReply!.ReplyId))
                    {
                        replyDtos.Insert(0, reReply!);
                    }

                    return ResponseDto.GetSuccess(new
                    {
                        comment,
                        replies = replyDtos.OrderBy(r => r.CreatedAt),
                        request.PageNumber,
                        request.PageSize,
                        totalReplies,
                        totalPages,
                        hasNextPage = request.PageNumber < totalPages,
                        hasPreviousPage = request.PageNumber > 1
                    }, "Replies retrieved successfully.");
                }
            }
            else
            {
                // Check comment in DB
                var isCommentExisted = await _commentRepo.ExistsAsync(c => c.CommentId == request.CommentId);
                if (!isCommentExisted) return ResponseDto.NotFound("Comment not found!");
            }

            // **Check if replies are cached**
            var cachedReplies = await _cacheService.GetAsync<List<ReplyDto>>(repliesCacheKey);
            if (cachedReplies != null)
            {
                var totalReplies = await _replyRepo.CountAsync(r => r.CommentId == request.CommentId && !r.IsDeleted);
                var totalPages = (int)Math.Ceiling((double)totalReplies / request.PageSize);

                return ResponseDto.GetSuccess(new
                {
                    comment,
                    replies = cachedReplies,
                    request.PageNumber,
                    request.PageSize,
                    totalReplies,
                    totalPages,
                    hasNextPage = request.PageNumber < totalPages,
                    hasPreviousPage = request.PageNumber > 1
                }, "Replies retrieved successfully (cached).");
            }

            // Fetch replies from DB if not cached
            var totalRepliesFromDb = await _replyRepo.CountAsync(r => r.CommentId == request.CommentId && !r.IsDeleted);
            var totalPagesFromDb = (int)Math.Ceiling((double)totalRepliesFromDb / request.PageSize);

            var repliesDto = await _replyRepo.FindAsyncWithPagingAndSorting(
                r => r.CommentId == request.CommentId && !r.IsDeleted,
                r => new ReplyDto
                {
                    ReplyId = r.ReplyId,
                    CommentId = r.CommentId,
                    CyclistId = r.CyclistId,
                    CyclistName = r.CyclistName,
                    Content = r.Content,
                    MediaFiles = r.MediaFiles.Select(mf => new MediaFileDto { MediaId = mf.MediaId, MediaUrl = mf.MediaUrl }).ToList(),
                    IsReacted = r.Reactions.Any(rr => rr.CyclistId == request.UserRequestId),
                    CreatedAt = r.CreatedAt,
                    LikesCount = r.LikesCount
                },
                request.PageNumber, 
                request.PageSize, 
                SortHelper.BuildSortExpression<Reply>("CreatedAt")
            );

            // Cache replies for 5 minutes
            await _cacheService.SetAsync(repliesCacheKey, repliesDto, TimeSpan.FromMinutes(5));

            return ResponseDto.GetSuccess(new
            {
                comment,
                replies = repliesDto,
                request.PageNumber,
                request.PageSize,
                totalReplies = totalRepliesFromDb,
                totalPages = totalPagesFromDb,
                hasNextPage = request.PageNumber < totalPagesFromDb,
                hasPreviousPage = request.PageNumber > 1
            }, "Replies retrieved successfully.");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }
}
