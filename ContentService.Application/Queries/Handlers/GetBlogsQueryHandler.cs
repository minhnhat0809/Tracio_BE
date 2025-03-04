using System.Linq.Expressions;
using ContentService.Application.DTOs.BlogDtos.Message;
using ContentService.Application.DTOs.BlogDtos.ViewDtos;
using ContentService.Application.DTOs.MediaFileDTOs.ViewDtos;
using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using ContentService.Domain.Enums;
using LinqKit;
using MediatR;
using Shared.Dtos;
using Shared.Ultilities;

namespace ContentService.Application.Queries.Handlers;

public class GetBlogsQueryHandler(
    IBlogRepo blogRepo, 
    IFollowerOnlyBlogRepo followerOnlyBlogRepo,
    IRabbitMqProducer rabbitMqProducer,
    IUserService userService,
    ICacheService cacheService) : IRequestHandler<GetBlogsQuery, ResponseDto>
{
    private readonly IBlogRepo _blogRepo = blogRepo;
    private readonly IFollowerOnlyBlogRepo _followerOnlyBlogRepo = followerOnlyBlogRepo;
    private readonly IRabbitMqProducer _rabbitMqProducer = rabbitMqProducer;
    private readonly IUserService _userService = userService;
    private readonly ICacheService _cacheService = cacheService;

    public async Task<ResponseDto> Handle(GetBlogsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.UserId.HasValue)
            {
                return await GetUserBlogs(request);
            }
            else
            {
                return await GetCachedPublicAndFollowerBlogs(request);
            }
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }

    private async Task<ResponseDto> GetCachedPublicAndFollowerBlogs(GetBlogsQuery request)
    {
        try
        {
            var publicBlogCacheKey = $"PublicBlogs:Page{request.PageNumber}:Size{request.PageSize}";
            var followerBlogCacheKey = $"FollowerBlogs:{request.UserRequestId}:Page{request.PageNumber}:Size{request.PageSize}";

            var cachedPublicBlogs = await _cacheService.GetAsync<List<BlogDtos>>(publicBlogCacheKey);
            var cachedFollowerBlogs = await _cacheService.GetAsync<List<BlogDtos>>(followerBlogCacheKey);

            if (cachedPublicBlogs == null)
            {
                cachedPublicBlogs = await FetchPublicBlogs(request);
                await _cacheService.SetAsync(publicBlogCacheKey, cachedPublicBlogs, TimeSpan.FromMinutes(10));
            }

            if (cachedFollowerBlogs == null)
            {
                cachedFollowerBlogs = await FetchFollowerOnlyBlogs(request);
                await _cacheService.SetAsync(followerBlogCacheKey, cachedFollowerBlogs, TimeSpan.FromMinutes(5));
            }
            
            // count
            var totalFollowerOnlyBlogs = await _followerOnlyBlogRepo.CountAsync(fb => fb.UserId == request.UserRequestId && !fb.IsRead);
            var totalPublicBlogs = await _blogRepo.CountAsync(PredicateBuilder.New<Blog>()
                .And(b => b.PrivacySetting == (sbyte)PrivacySetting.Public && b.Status == (sbyte)BlogStatus.Published));

            var finalBlogs = InterleaveLists(cachedFollowerBlogs, cachedPublicBlogs).Take(request.PageSize).ToList();

            // **Trigger RabbitMQ Event for unread follower blogs**
            if (cachedFollowerBlogs.Count > 0)
            {
                await _rabbitMqProducer.PublishAsync(new MarkBlogsAsReadEvent
                {
                    UserId = request.UserRequestId,
                    BlogIds = cachedFollowerBlogs.Select(b => b.BlogId).ToList()
                });
            }

            var totalBlogs = totalFollowerOnlyBlogs + totalPublicBlogs;
            var totalPages = (int)Math.Ceiling((double)totalBlogs / request.PageSize);

            var response = new
            {
                blogs = finalBlogs,
                request.PageNumber,
                request.PageSize,
                totalBlogs,
                totalPages,
                hasNextPage = request.PageNumber < totalPages,
                hasPreviousPage = request.PageNumber > 1
            };

            return ResponseDto.GetSuccess(response, "Blogs retrieved successfully!");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }

    private async Task<List<BlogDtos>> FetchPublicBlogs(GetBlogsQuery request)
    {
        var basePredicate = PredicateBuilder.New<Blog>(true)
            .And(b => b.PrivacySetting == (sbyte)PrivacySetting.Public && b.Status == (sbyte)BlogStatus.Published);

        if (request.CategoryId.HasValue)
            basePredicate = basePredicate.And(b => b.CategoryId == request.CategoryId);

        var sortExpression = SortHelper.BuildSortExpression<Blog>(GetSortByField(request.SortBy));

        return await _blogRepo.FindAsyncWithPagingAndSorting(
            basePredicate,
            SelectBlogDtos(request.UserRequestId),
            request.PageNumber, request.PageSize,
            sortExpression, request.Ascending,
            b => b.MediaFiles, b => b.Reactions
        );
    }

    private async Task<List<BlogDtos>> FetchFollowerOnlyBlogs(GetBlogsQuery request)
    {
        var sortExpression = SortHelper.BuildSortExpression<UserBlogFollowerOnly>("CreatedAt");

        return await _followerOnlyBlogRepo.FindAsyncWithPagingAndSorting(
            b => b.UserId == request.UserRequestId && !b.IsRead,
            SelectFollowerOnlyBlogDtos(request.UserRequestId),
            request.PageNumber, request.PageSize,
            sortExpression, false,
            b => b.Blog, b => b.Blog.MediaFiles, b => b.Blog.Reactions
        );
    }

    private async Task<ResponseDto> GetUserBlogs(GetBlogsQuery request)
    {
        var basePredicate = PredicateBuilder.New<Blog>(true);
        var result = await _userService.CheckUserFollower(request.UserId!.Value, request.UserRequestId);
        
        if (!result.IsExisted) return ResponseDto.NotFound("User not found");

        basePredicate = basePredicate.And(b => b.CreatorId == request.UserId);

        if (request.UserRequestId != request.UserId.Value)
        {
            basePredicate = basePredicate.And(b => b.Status == (sbyte)BlogStatus.Published);
            basePredicate = result.IsFollower
                ? basePredicate.And(b => b.Status != (sbyte)PrivacySetting.Private)
                : basePredicate.And(b => b.PrivacySetting == (sbyte)PrivacySetting.Public);
        }
        else
        {
            // user view their own profile, check status of blog user want to view
            if (request.Status.HasValue)
            {
                if (IsValidBlogStatus(request.Status.Value))
                {
                    basePredicate = basePredicate.And(b => b.Status == request.Status.Value);
                }
            }
            else
            {
                basePredicate = basePredicate.And(b => b.Status == (sbyte)BlogStatus.Published);
            }
        }

        request.SortBy = GetSortByField(request.SortBy);
        var sortExpression = SortHelper.BuildSortExpression<Blog>(request.SortBy);

        var blogs = await _blogRepo.FindAsyncWithPagingAndSorting(
            basePredicate,
            SelectBlogDtos(request.UserRequestId),
            request.PageNumber, request.PageSize,
            sortExpression, request.Ascending,
            b => b.MediaFiles, b => b.Reactions
        );

        var totalBlogs = await _blogRepo.CountAsync(basePredicate);
        var totalPages = (int)Math.Ceiling((double)totalBlogs / request.PageSize);

        var response = new
        {
            blogs,
            request.PageNumber,
            request.PageSize,
            totalBlogs,
            totalPages,
            hasNextPage = request.PageNumber < totalPages,
            hasPreviousPage = request.PageNumber > 1,
        };

        return ResponseDto.GetSuccess(response, "Blogs retrieved successfully!");
    }

    private static List<BlogDtos> InterleaveLists(List<BlogDtos> list1, List<BlogDtos> list2)
    {
        var result = new List<BlogDtos>();
        int i = 0, j = 0;

        while (i < list1.Count || j < list2.Count)
        {
            if (i < list1.Count) result.Add(list1[i++]);
            if (j < list2.Count) result.Add(list2[j++]);
        }

        return result;
    }

    private static string GetSortByField(string sortBy)
    {
        return sortBy.ToLower() switch
        {
            "mostlike" => "ReactionsCount",
            "mostcomment" => "CommentsCount",
            _ => "CreatedAt"
        };
    }

    private static Expression<Func<Blog, BlogDtos>> SelectBlogDtos(int? userRequestId)
    {
        return b => new BlogDtos
        {
            BlogId = b.BlogId,
            UserId = b.CreatorId,
            UserName = b.CreatorName,
            Avatar = b.CreatorAvatar,
            PrivacySetting = b.PrivacySetting,
            Content = b.Content,
            MediaFiles = b.MediaFiles.Select(mf => new MediaFileDto { MediaId = mf.MediaId, MediaUrl = mf.MediaUrl }).ToList(),
            CreatedAt = b.CreatedAt,
            UpdatedAt = b.UpdatedAt,
            LikesCount = b.ReactionsCount,
            CommentsCount = b.CommentsCount,
            IsReacted = b.Reactions.Any(r => r.CyclistId == userRequestId),
            IsBookmarked = b.BlogBookmarks.Any(bm => bm.OwnerId == userRequestId)
        };
    }
    
    private static Expression<Func<UserBlogFollowerOnly, BlogDtos>> SelectFollowerOnlyBlogDtos(int? userRequestId)
    {
        return b => new BlogDtos
        {
            BlogId = b.BlogId,
            UserId = b.Blog.CreatorId,
            UserName = b.Blog.CreatorName,
            Avatar = b.Blog.CreatorAvatar,
            PrivacySetting = b.Blog.PrivacySetting,
            Content = b.Blog.Content,
            MediaFiles = b.Blog.MediaFiles.Select(mf => new MediaFileDto { MediaId = mf.MediaId, MediaUrl = mf.MediaUrl }).ToList(),
            CreatedAt = b.Blog.CreatedAt,
            UpdatedAt = b.Blog.UpdatedAt,
            LikesCount = b.Blog.ReactionsCount,
            CommentsCount = b.Blog.CommentsCount,
            IsReacted = b.Blog.Reactions.Any(r => r.CyclistId == userRequestId),
            IsBookmarked = b.Blog.BlogBookmarks.Any(bm => bm.OwnerId == userRequestId)
        };
    }
    
    private static bool IsValidBlogStatus(sbyte status) => Enum.IsDefined(typeof(BlogStatus), status);
}
