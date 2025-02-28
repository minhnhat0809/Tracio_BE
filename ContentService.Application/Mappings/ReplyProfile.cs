using AutoMapper;
using ContentService.Application.Commands;
using ContentService.Domain.Entities;

namespace ContentService.Application.Mappings;

public class ReplyProfile : Profile
{
    public ReplyProfile()
    {
        CreateMap<CreateReplyCommand, Reply>()
            .ForMember(dest => dest.MediaFiles, opt => opt.Ignore());
    }
}