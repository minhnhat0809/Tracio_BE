using AutoMapper;
using ContentService.Application.Interfaces;
using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Commands.Handlers;

public class CreateCommentCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICommentRepo commentRepo) 
    : IRequestHandler<CreateCommentCommand, ResponseDto>
{
    private readonly ICommentRepo _commentRepo = commentRepo;
    
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    
    private readonly IMapper _mapper = mapper;
    
    public async Task<ResponseDto> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            return ResponseDto.CreateSuccess(null, "Comment created successfully!");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }
}