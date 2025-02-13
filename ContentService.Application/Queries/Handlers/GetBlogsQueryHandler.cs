using ContentService.Application.DTOs.BlogDtos.ViewDtos;
using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using ContentService.Domain.Enums;
using LinqKit;
using MediatR;
using Shared.Dtos;
using Shared.Ultilities;

namespace ContentService.Application.Queries.Handlers;

public class GetBlogsQueryHandler(IBlogRepo blogRepo, IUserService userService) : IRequestHandler<GetBlogsQuery, ResponseDto>
{
    private readonly IBlogRepo _blogRepo = blogRepo;

    private readonly IUserService _userService = userService;
    
    public async Task<ResponseDto> Handle(GetBlogsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var basePredicate = PredicateBuilder.New<Blog>(true);

            if (request.UserId.HasValue)
            {
                basePredicate = basePredicate.And(b => b.CreatorId.Equals(request.UserId));
                
                if (request.UserRequestId != request.UserId.Value)
                {
                    // check if user request and author is follower
                    var isFollower = await _userService.IsFollower(request.UserId.Value, request.UserRequestId);
                    
                    basePredicate = basePredicate.And(b => b.Status == (sbyte) BlogStatus.Published);
                    
                    basePredicate = isFollower ? basePredicate.And(b => b.Status != (sbyte) PrivacySetting.Private) :
                        basePredicate.And(b => b.PrivacySetting == (sbyte) PrivacySetting.Public);
                }
            }
            else
            {
                return await AdvancedFilterBlog(request.UserRequestId, request.PageNumber, request.PageSize, request.SortBy, request.Ascending);
            }
            
            if (string.IsNullOrWhiteSpace(request.SortBy))
            {
                request.SortBy = "CreatedAt";
            }
            
            // count blogs
            var total = await _blogRepo.CountAsync(basePredicate);
            if (total == 0) return ResponseDto.GetSuccess(new
            {
                blogs = new List<BlogDtos>(),
                total,
                pageNumber = request.PageNumber,
                pageSize = request.PageSize
            } ,
                "No blogs found");
            
            // build sort expression
            var sortExpression = SortHelper.BuildSortExpression<Blog>(request.SortBy);

            // fetch blogs
            var blogs = await _blogRepo.FindAsyncWithPagingAndSorting(
                basePredicate,
                b => new BlogDtos()
                {
                    BlogId = b.BlogId,
                    UserId = b.CreatorId,
                    UserName = b.CreatorName,
                    Avatar = b.CreatorAvatar,
                    PrivacySetting = b.PrivacySetting,
                    Content = b.Content,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt,
                    LikesCount = b.ReactionsCount,
                    CommentsCount = b.CommentsCount
                },
                request.PageNumber, request.PageSize,
                sortExpression, request.Ascending
            );
            
            return ResponseDto.GetSuccess(new
                {
                    blogs,
                    total,
                    pageNumber = request.PageNumber,
                    pageSize = request.PageSize
                },
                "Blogs retrieved successfully!");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }

    private async Task<ResponseDto> AdvancedFilterBlog(int userId, int pageSize, int pageNumber, string? sortBy, bool ascending)
    {
        try
        {
            var fetchSize = pageSize * 2; 
            var finalBlogs = new List<BlogDtos>();
            
            var basePredicate = PredicateBuilder.New<Blog>(true);

            while (finalBlogs.Count < pageSize)
            {
                basePredicate = basePredicate.And(b => b.Status == (sbyte) BlogStatus.Published);
                
                basePredicate = basePredicate.And(b => b.PrivacySetting != (sbyte) PrivacySetting.Private);
                
                // count blogs
                var total = await _blogRepo.CountAsync(basePredicate);
                if (total == 0) return ResponseDto.GetSuccess(new
                    {
                        blogs = new List<BlogDtos>(),
                        total,
                        pageNumber,
                        pageSize
                    } ,
                    "No blogs found");
                
                if (string.IsNullOrWhiteSpace(sortBy))
                {
                    sortBy = "CreatedAt";
                }
                
                // build sort expression
                var sortExpression = SortHelper.BuildSortExpression<Blog>(sortBy);
                
                // fetch blogs
                var rawBlogs  = await _blogRepo.FindAsyncWithPagingAndSorting(
                    basePredicate,
                    b => new BlogDtos()
                    {
                        BlogId = b.BlogId,
                        UserId = b.CreatorId,
                        UserName = b.CreatorName,
                        Avatar = b.CreatorAvatar,
                        PrivacySetting = b.PrivacySetting,
                        Content = b.Content,
                        CreatedAt = b.CreatedAt,
                        UpdatedAt = b.UpdatedAt,
                        LikesCount = b.ReactionsCount,
                        CommentsCount = b.CommentsCount
                    },
                    pageNumber, fetchSize,
                    sortExpression, ascending
                );
                
                if (rawBlogs.Count == 0)
                    break;
                
                var followerOnlyAuthors = rawBlogs
                    .Where(b => b.PrivacySetting == (sbyte) PrivacySetting.FollowerOnly)
                    .Select(b => b.UserId)
                    .Distinct()
                    .ToList();
                
                // Call UserService gRPC (batch check for follow status)
                var followingAuthors = new List<int>();
                if (followerOnlyAuthors.Count != 0)
                {
                    followingAuthors = await _userService.CheckFollowings(userId, followerOnlyAuthors);
                }
                
                var filteredBlogs = rawBlogs
                    .Where(b => b.PrivacySetting != (sbyte) PrivacySetting.FollowerOnly || followingAuthors.Contains(b.UserId))
                    .ToList();
                
                var remainingSlots = pageSize - finalBlogs.Count;
                finalBlogs.AddRange(filteredBlogs.Take(remainingSlots));
                
                if (finalBlogs.Count < pageSize)
                {
                    pageNumber++;
                }
                else
                {
                    break; 
                }
            }
            
            return ResponseDto.GetSuccess(new
            {
                blogs = finalBlogs,
                pageNumber,
                pageSize,
            }, "Blogs retrieved successfully!");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }
}