using AutoMapper;
using ContentService.Application.Interfaces;
using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Commands.Handlers;

public class CreateBlogCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IBlogRepo blogRepo) : IRequestHandler<CreateBlogCommand, ResponseDto>
{
    private readonly IBlogRepo _blogRepo = blogRepo;
    
    private readonly IMapper _mapper = mapper;
    
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    
    public async Task<ResponseDto> Handle(CreateBlogCommand request, CancellationToken cancellationToken)
    {
        try
        {
            return ResponseDto.CreateSuccess(null, "Blog created successfully!");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }
}