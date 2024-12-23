﻿using AutoMapper;
using ContentService.Application.DTOs.CommentDtos.ViewDtos;
using ContentService.Domain.Entities;

namespace ContentService.Application.Mappings;

public class CommentProfile : Profile
{
    public CommentProfile()
    {
        CreateMap<Comment, CommentDto>();
    }
}