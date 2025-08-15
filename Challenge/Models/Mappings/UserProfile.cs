using AutoMapper;
using Challenge.Models.DTOs.Auth;
using Challenge.Models.Entities.User;


namespace Challenge.Models.Mappings
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<RegisterRequest, User>()
            .ForMember(dest => dest.Password, opt => opt.Ignore())
            .ForMember(dest => dest.RefreshToken, opt => opt.Ignore());
        }
    }
}