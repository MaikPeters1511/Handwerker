using AutoMapper;
using FS.Keycloak.RestApiClient.Model;
using Handwerker.Application.Services.Keycloak.Models;

namespace Handwerker.Application;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<UserRepresentation, KcUser>(MemberList.Destination)
            .ForMember(dst => dst.Attributes, opt => opt.MapFrom(src => src.Attributes));
        
        CreateMap<KcUser, UserRepresentation>()
            .ForMember(dst => dst.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dst => dst.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dst => dst.Username, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dst => dst.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dst => dst.Enabled, opt => opt.MapFrom(src => src.Enabled))
            .ForMember(dst => dst.Attributes, opt => opt.MapFrom(src => src.Attributes))
            .ForMember(dst => dst.Id, opt => opt.Ignore())
            .ForMember(dst => dst.EmailVerified, opt => opt.Ignore());
        
        CreateMap<RoleRepresentation, KcRole>(MemberList.Destination);
        
        CreateMap<KcRole, RoleRepresentation>()
            .ForMember(dst => dst.Id, opt => opt.Ignore())
            .ForMember(dst => dst.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dst => dst.Description, opt => opt.MapFrom(src => src.Description));
        
        CreateMap<KcUserDto, UserRepresentation>()
            .ForMember(dst => dst.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dst => dst.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dst => dst.Username, opt => opt.MapFrom(src => src.UserName))
            .ForMember(dst => dst.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dst => dst.Enabled, opt => opt.MapFrom(src => src.Enabled))
            .ForMember(dst => dst.Attributes, opt => opt.MapFrom(src => src.Attributes))
            .ForMember(src => src.Id, opt => opt.Ignore())
            .ForMember(src => src.EmailVerified, opt => opt.Ignore());
    }
}