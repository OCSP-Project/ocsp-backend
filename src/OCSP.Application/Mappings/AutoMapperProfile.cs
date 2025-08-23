
using AutoMapper;
using OCSP.Application.DTOs.Auth;
using OCSP.Application.DTOs.Profile;
using OCSP.Domain.Entities;

namespace OCSP.Application.Mappings
{
    public class AutoMapperProfile : AutoMapper.Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserResponseDto>();
            CreateMap<RegisterDto, User>();
            
            // Profile mappings
            CreateMap<OCSP.Domain.Entities.Profile, ProfileDto>();
            CreateMap<OCSP.Domain.Entities.ProfileDocument, ProfileDocumentDto>();
        }
    }
}