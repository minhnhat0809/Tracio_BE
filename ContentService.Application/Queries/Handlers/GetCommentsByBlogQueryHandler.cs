using AutoMapper;
using ContentService.Application.DTOs.BlogDtos.ViewDtos;
using ContentService.Application.DTOs.CommentDtos.ViewDtos;
using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using LinqKit;
using MediatR;
using Shared.Dtos;
using Shared.Ultilities;

namespace ContentService.Application.Queries.Handlers;

public class GetCommentsByBlogQueryHandler(IBlogRepo blogRepo, ICommentRepo commentRepo, IMapper mapper) : IRequestHandler<GetCommentsByBlogQuery, ResponseDto>
{
    private readonly IMapper _mapper = mapper;
    
    private readonly IBlogRepo _blogRepo = blogRepo;
    
    private readonly ICommentRepo _commentRepo = commentRepo;
    
    public async Task<ResponseDto> Handle(GetCommentsByBlogQuery request, CancellationToken cancellationToken)
    {
        try
        {
            if (request.BlogId <= 0) return ResponseDto.BadRequest("Blog Id is required");
            
            // check blog in db
            var blog = await _blogRepo.GetByIdAsync(b => b.BlogId.Equals(request.BlogId), b => b);

            if (blog == null) return ResponseDto.NotFound($"Blog not found with this id : {request.BlogId}");
            
            var basePredicate = PredicateBuilder.New<Comment>(true);
            
            // build filter expression
            basePredicate = basePredicate
                .And(c => c.BlogId.Equals(request.BlogId));
            
            // build sort expression
            var sortExpression = SortHelper.BuildSortExpression<Comment>("CreatedAt");
            
            // count comments 
            var total = await _commentRepo.CountAsync(basePredicate);
            if (total == 0) return ResponseDto.GetSuccess(new
                {
                    comments = new List<CommentDto>(), 
                    count = total, 
                    pageNumber = request.PageNumber, 
                    pageSize = request.PageSize
                }, 
                "Comments retrieved successfully!");
            
            // map blog
            var blogDto = _mapper.Map<BlogWithCommentsDto>(blog);
            
            // fetch comments
            var commentsDto = await _commentRepo.FindAsyncWithPagingAndSorting(
                basePredicate,
                c => new CommentDto()
                {
                    CommentId = c.CommentId,
                    UserId = c.CyclistId,
                    UserName = c.CyclistName,
                    Avatar = c.CyclistAvatar,
                    Content = c.Content,
                    CreatedAt = c.CreatedAt,
                    UpdatedAt = c.UpdatedAt,
                    IsEdited = c.IsEdited,
                    LikesCount = c.LikesCount
                },
                request.PageNumber, request.PageSize,
                sortExpression, request.IsAscending
                );

            // map comments to blogDto
            blogDto.Comments = commentsDto;
            
            return ResponseDto.GetSuccess(new
                {
                    blog = blogDto, 
                    total = 0, 
                    pageNumber = request.PageNumber, 
                    pageSize = request.PageSize
                }, 
                "Comments of the blog retrieved successfully!");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }
}