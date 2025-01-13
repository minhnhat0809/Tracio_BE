using AutoMapper;
using ContentService.Application.Interfaces;
using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Commands.Handlers;

public class UpdateBlogCommandHandler(IMapper mapper, IBlogRepo blogRepo) : IRequestHandler<UpdateBlogCommand, ResponseDto>
{
    
    private readonly IBlogRepo _blogRepo = blogRepo;
    
    private readonly IMapper _mapper = mapper;
    
    public async Task<ResponseDto> Handle(UpdateBlogCommand request, CancellationToken cancellationToken)
    {
        try
        {
            
            return ResponseDto.UpdateSuccess(null, "Blog updated successfully!");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }
}