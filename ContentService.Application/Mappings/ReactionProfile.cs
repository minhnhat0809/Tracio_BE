using AutoMapper;
using ContentService.Application.DTOs.ReactionDtos.ViewDtos;
using ContentService.Domain.Entities;

namespace ContentService.Application.Mappings;

public class ReactionProfile : Profile
{
    public ReactionProfile()
    {
        CreateMap<Reaction, ReactionDto>();
    }
}