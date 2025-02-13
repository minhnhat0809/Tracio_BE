using AutoMapper;
using ContentService.Application.Interfaces;
using ContentService.Domain.Entities;
using ContentService.Domain.Enums;
using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Commands.Handlers;

public class CreateBookmarkCommandHandler(IBlogRepo blogRepo, IBookmarkRepo bookmarkRepo, IMapper mapper, IUserService userService) : IRequestHandler<CreateBookmarkCommand, ResponseDto>
{
    private readonly IBookmarkRepo _bookmarkRepo = bookmarkRepo;
    
    private readonly IBlogRepo _blogRepo = blogRepo;
    
    private readonly IMapper _mapper = mapper;
    
    private readonly IUserService _userService = userService;
    
    public async Task<ResponseDto> Handle(CreateBookmarkCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // check blog in db
            var isBlogExisted = await _blogRepo.ExistsAsync(b => b.BlogId == request.BlogId && b.Status != (sbyte) BlogStatus.Deleted);
            if (!isBlogExisted) return ResponseDto.NotFound("Blog not found");
            
            // check userId and get user's name
            var userDto = await _userService.ValidateUser(request.OwnerId);
            if (!userDto.IsUserValid) return ResponseDto.NotFound("User does not exist");
            
            var bookMark = _mapper.Map<BlogBookmark>(request);
            
            var isBookmarkSuccess = await _bookmarkRepo.CreateAsync(bookMark);
            
            return isBookmarkSuccess ? ResponseDto.CreateSuccess(null, "Bookmark blog successfully!") : 
                    ResponseDto.InternalError("Bookmark blog creation failed");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }
}