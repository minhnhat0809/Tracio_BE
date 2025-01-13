using AutoMapper;
using ContentService.Application.Interfaces;
using MediatR;
using Shared.Dtos;

namespace ContentService.Application.Commands.Handlers;

public class UpdateCommentCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, ICommentRepo commentRepo) : IRequestHandler<UpdateCommentCommand, ResponseDto>
{
    private readonly IMapper _mapper = mapper;
    
    private readonly ICommentRepo _commentRepo = commentRepo;
    
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    
    public async Task<ResponseDto> Handle(UpdateCommentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            return ResponseDto.UpdateSuccess(null, "Comment updated successfully!");
        }
        catch (Exception e)
        {
            return ResponseDto.InternalError(e.Message);
        }
    }
}