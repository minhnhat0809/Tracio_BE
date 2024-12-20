using AutoMapper;
using ContentService.Application.DTOs;
using ContentService.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace ContentService.Application.Queries.Handlers;

public class GetBlogsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IBlogRepo blogRepo) : IRequestHandler<GetBlogsQuery, ResponseDto>
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    
    private readonly IMapper _mapper = mapper;
    
    private readonly IBlogRepo _blogRepo = blogRepo;

    public async Task<ResponseDto> Handle(GetBlogsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return new ResponseDto(null, "", true, StatusCodes.Status200OK);
        }
        catch (Exception e)
        {
            return new ResponseDto(null, e.Message, false, StatusCodes.Status500InternalServerError);
        }
    }
}