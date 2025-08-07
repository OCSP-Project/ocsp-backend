
using AutoMapper;
using OCSP.Application.DTOs.Auth;
using OCSP.Domain.Entities;

namespace OCSP.Application.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserResponseDto>();
            CreateMap<RegisterDto, User>();
        }
    }
}