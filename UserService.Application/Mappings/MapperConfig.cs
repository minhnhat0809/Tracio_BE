using AutoMapper;
using UserService.Application.DTOs.Auths;
using UserService.Application.DTOs.Roles;
using UserService.Application.DTOs.Sessions;
using UserService.Application.DTOs.Users;
using UserService.Domain.Entities;

namespace UserService.Application.Mappings;

public class MapperConfig : Profile
{
    public MapperConfig()
    {
        // Map User to LoginResponseViewModel
        CreateMap<User, LoginViewModel>()
            .ForMember(dest => dest.Session, opt => opt.Ignore()); // Session will be mapped separately

        // Map UserSession to SessionViewModel
        CreateMap<UserSession, SessionViewModel>();

        // Map User
        CreateMap<User, UserViewModel>(); 
    }
    
}