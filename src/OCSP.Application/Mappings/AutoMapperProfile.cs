
using AutoMapper;
using OCSP.Application.DTOs.Auth;
using OCSP.Application.DTOs.Profile;
using OCSP.Application.DTOs.Project;
using OCSP.Application.DTOs.ProjectDailyResource;
using OCSP.Application.DTOs.ProgressMedia;
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
            
            // Project mappings
            CreateMap<Project, ProjectResponseDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.SupervisorName, opt => opt.MapFrom(src => src.Supervisor != null ? src.Supervisor.User!.Username : null))
                .ForMember(dest => dest.HomeownerName, opt => opt.MapFrom(src => src.Homeowner != null ? src.Homeowner.Username : null));

            // ProgressMedia mappings
            CreateMap<ProgressMedia, ProgressMediaDto>()
                .ForMember(dest => dest.CreatorName, opt => opt.MapFrom(src => src.Creator != null ? src.Creator.Username : "Unknown"));

            // ProjectDailyResource mappings
            CreateMap<CreateProjectDailyResourceDto, ProjectDailyResource>();
            CreateMap<UpdateProjectDailyResourceDto, ProjectDailyResource>();
            CreateMap<ProjectDailyResource, ProjectDailyResourceDto>()
                .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project != null ? src.Project.Name : "Unknown"));
        }
    }
}