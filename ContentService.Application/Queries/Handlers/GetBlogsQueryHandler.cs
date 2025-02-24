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
    IUserService userService) : IRequestHandler<GetBlogsQuery, ResponseDto>
{
    private readonly IBlogRepo _blogRepo = blogRepo;
    private readonly IFollowerOnlyBlogRepo _followerOnlyBlogRepo = followerOnlyBlogRepo;
    private readonly IRabbitMqProducer _rabbitMqProducer = rabbitMqProducer;
    private readonly IUserService _userService = userService;

    public async Task<ResponseDto> Handle(GetBlogsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var basePredicate = PredicateBuilder.New<Blog>(true);

            if (request.CategoryId.HasValue)
                basePredicate = basePredicate.And(b => b.CategoryId == request.CategoryId);

            if (request.UserId.HasValue)
            {
                var result = await _userService.CheckUserFollower(request.UserId.Value, request.UserRequestId);
                if (!result.IsExisted) return ResponseDto.NotFound("User not found");

                basePredicate = basePredicate.And(b => b.CreatorId == request.UserId);

                if (request.UserRequestId != request.UserId.Value)
                {
                    basePredicate = basePredicate.And(b => b.Status == (sbyte)BlogStatus.Published);
                    basePredicate = result.IsFollower
                        ? basePredicate.And(b => b.Status != (sbyte)PrivacySetting.Private)
                        : basePredicate.And(b => b.PrivacySetting == (sbyte)PrivacySetting.Public);
                }
            }
            else
            {
                return await AdvancedFilterBlog(request);
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

            // count blogs
            var totalBlogs = await _blogRepo.CountAsync(basePredicate);
            
            var totalPages = (request.PageSize / totalBlogs) +1;

            return ResponseDto.GetSuccess(new
            {
                blogs, 
                request.PageNumber, 
                request.PageSize,
                totalBlogs,
                totalPages,
                hasNextPage = request.PageNumber < totalPages,
                hasPreviousPage = request.PageNumber > 1,
            }, "Blogs retrieved successfully!");
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
            var estimatedFollowerSize = (int) Math.Ceiling(request.PageSize / 2.0);

            var basePredicate = PredicateBuilder.New<Blog>(true)
                .And(b => b.PrivacySetting == (sbyte)PrivacySetting.Public && b.Status == (sbyte)BlogStatus.Published);

            if (request.CategoryId.HasValue)
                basePredicate = basePredicate.And(b => b.CategoryId == request.CategoryId);
            
            request.SortBy = GetSortByField(request.SortBy);
            var sortExpressionBlog = SortHelper.BuildSortExpression<Blog>(request.SortBy);
            var sortExpressionFollowerOnly = SortHelper.BuildSortExpression<UserBlogFollowerOnly>("CreatedAt");
            
            // fetch followerOnly blogs
            var followerOnlyBlogs = await _followerOnlyBlogRepo.FindAsyncWithPagingAndSorting(
                b => b.UserId == request.UserRequestId && !b.IsRead,
                SelectFollowerOnlyBlogDtos(request.UserRequestId),
                request.PageNumber, estimatedFollowerSize,
                sortExpressionFollowerOnly, false,
                b => b.Blog, b => b.Blog.MediaFiles, b => b.Blog.Reactions
            );
            
            // count followerOnly blogs
            var totalFollowerOnlyBlogs = await _followerOnlyBlogRepo.CountAsync(fb => fb.UserId == request.UserRequestId && !fb.IsRead);

            var estimatedPublicSize = request.PageSize - followerOnlyBlogs.Count;

            var publicBlogs = await _blogRepo.FindAsyncWithPagingAndSorting(
                basePredicate,
                SelectBlogDtos(request.UserRequestId),
                request.PageNumber, estimatedPublicSize,
                sortExpressionBlog, request.Ascending,
                b => b.MediaFiles, b => b.Reactions
            );
            
            // count public blogs
            var totalPublicBlogs = await _blogRepo.CountAsync(basePredicate);
            
            var totalBlogs = totalPublicBlogs + totalFollowerOnlyBlogs;
            
            var totalPages = (request.PageSize / totalBlogs) +1;

            var finalBlogs = InterleaveLists(followerOnlyBlogs, publicBlogs).Take(request.PageSize).ToList();

            if (followerOnlyBlogs.Count != 0)
            {
                await _rabbitMqProducer.PublishAsync(new MarkBlogsAsReadEvent
                {
                    UserId = request.UserRequestId,
                    BlogIds = followerOnlyBlogs.Select(b => b.BlogId).ToList()
                }, "mark-blogs-as-read");
            }

            return ResponseDto.GetSuccess(new
                {
                    blogs = finalBlogs, 
                    request.PageNumber, 
                    request.PageSize,
                    totalBlogs,
                    totalPages,
                    hasNextPage = request.PageNumber < totalPages,
                    hasPreviousPage = request.PageNumber > 1
                },
                "Blogs retrieved successfully!");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
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
            ReactionId = b.Reactions.Where(r => r.CyclistId == userRequestId).Select(r => r.ReactionId).FirstOrDefault()
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
            ReactionId = b.Blog.Reactions.Where(r => r.CyclistId == userRequestId).Select(r => r.ReactionId).FirstOrDefault()
        };
    }
}
