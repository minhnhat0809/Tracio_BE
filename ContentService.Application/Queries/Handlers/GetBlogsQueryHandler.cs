using ContentService.Application.DTOs.BlogDtos.ViewDtos;
using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using ContentService.Domain.Enums;
using LinqKit;
using MediatR;
using Shared.Dtos;
using Shared.Ultilities;

namespace ContentService.Application.Queries.Handlers;

public class GetBlogsQueryHandler(IBlogRepo blogRepo, IFollowerOnlyBlogRepo followerOnlyBlogRepo, IUserService userService) : IRequestHandler<GetBlogsQuery, ResponseDto>
{
    private readonly IBlogRepo _blogRepo = blogRepo;
    
    private readonly IFollowerOnlyBlogRepo _followerOnlyBlogRepo = followerOnlyBlogRepo;

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
                    
                    // fetch only published blogs
                    basePredicate = basePredicate.And(b => b.Status == (sbyte) BlogStatus.Published);
                    
                    // fetch followerOnly if they follow each other
                    basePredicate = isFollower ? basePredicate.And(b => b.Status != (sbyte) PrivacySetting.Private) :
                        basePredicate.And(b => b.PrivacySetting == (sbyte) PrivacySetting.Public);
                }
            }
            else
            {
                return await AdvancedFilterBlog(request);
            }
            
            if (string.IsNullOrWhiteSpace(request.SortBy))
            {
                request.SortBy = "CreatedAt";
            }
            
            // build sort expression
            var sortExpression = SortHelper.BuildSortExpression<Blog>(request.SortBy);

            // fetch blogs
            var blogs = await _blogRepo.FindAsyncWithPagingAndSorting(
                basePredicate,
                b => new BlogDtos
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

    private async Task<ResponseDto> AdvancedFilterBlog(GetBlogsQuery request)
    
    {
        try
        {
            var halfPageSize = (int)Math.Ceiling(request.PageSize / 2.0);
            
            var basePredicate = PredicateBuilder.New<Blog>(true);

            basePredicate = basePredicate.And(b => b.PrivacySetting == (sbyte)PrivacySetting.Public &&
                                                   b.Status == (sbyte)BlogStatus.Published);
            
            if (string.IsNullOrWhiteSpace(request.SortBy))
            {
                request.SortBy = "CreatedAt";
            }
            
            // build sort expression
            var sortExpressionBlog = SortHelper.BuildSortExpression<Blog>(request.SortBy);
            
            // fetch blogs
            var publicBlogs = await _blogRepo.FindAsyncWithPagingAndSorting(
                basePredicate,
                b => new BlogDtos
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
                request.PageNumber, halfPageSize,
                sortExpressionBlog, request.Ascending
            );
            
            // fetch followerOnly blogs of user
            var basePredicateFollowerOnly = PredicateBuilder.New<UserBlogFollowerOnly>(true);

            // build sort expression
            var sortExpressionFollowerOnly = SortHelper.BuildSortExpression<UserBlogFollowerOnly>("CreatedAt");

            var followerOnlyBlogs = await _followerOnlyBlogRepo.FindAsyncWithPagingAndSorting(b => b.UserId == request.UserRequestId, b => 
                new BlogDtos
                {
                    BlogId = b.BlogId,
                    UserId = b.Blog.CreatorId,
                    UserName = b.Blog.CreatorName,
                    Avatar = b.Blog.CreatorAvatar,
                    PrivacySetting = b.Blog.PrivacySetting,
                    Content = b.Blog.Content,
                    CreatedAt = b.Blog.CreatedAt,
                    UpdatedAt = b.Blog.UpdatedAt,
                    LikesCount = b.Blog.ReactionsCount,
                    CommentsCount = b.Blog.CommentsCount
                },
                request.PageNumber, halfPageSize,
                sortBy: sortExpressionFollowerOnly, ascending: false,
                includes: b => b.Blog);
            
            // Merge & Interleave the Blogs
            var finalBlogs = InterleaveLists(followerOnlyBlogs, publicBlogs);
            
            return ResponseDto.GetSuccess(new
            {
                blogs = new List<BlogDtos>(),
                pageNumber = request.PageNumber,
                pageSize = request.PageSize,
            }, "Blogs retrieved successfully!");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }
    
    // Helper method to interleave two lists
    private static List<BlogDtos> InterleaveLists(List<BlogDtos> list1, List<BlogDtos> list2)
    {
        var result = new List<BlogDtos>();
        int i = 0, j = 0;

        while (i < list1.Count || j < list2.Count)
        {
            if (i < list1.Count)
                result.Add(list1[i++]); // Add a FollowerOnly blog

            if (j < list2.Count)
                result.Add(list2[j++]); // Add a Public blog
        }

        return result;
    }
}