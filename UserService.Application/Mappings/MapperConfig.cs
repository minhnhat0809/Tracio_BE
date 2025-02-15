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
        CreateMap<User, UserViewModel>()
            .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => 
                src.Role.Length >= 4 
                    ? ConvertRoleToName(src.Role) 
                    : "Unknown"
            ));
        CreateMap<UpdateUserProfileModel, User>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

    }
    private static string ConvertRoleToName(byte[] role)
    {
        int roleInt = BitConverter.ToInt32(role, 0); // Convert byte[] to int

        return roleInt switch
        {
            256 => "cyclist",
            512 => "shop_owner",
            _ => "Unknown Role"
        };
    }
}