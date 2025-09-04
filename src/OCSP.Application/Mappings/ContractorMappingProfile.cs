using System.Linq;
using AutoMapper;
using OCSP.Application.DTOs.Contractor;
using OCSP.Domain.Entities;

namespace OCSP.Application.Mappings
{
    // Kế thừa rõ ràng AutoMapper.Profile để tránh trùng tên
    public class ContractorMappingProfile : AutoMapper.Profile
    {
        public ContractorMappingProfile()
        {
            CreateMap<Contractor, ContractorSummaryDto>()
                .ForMember(dest => dest.Specialties, opt => opt.MapFrom(src => src.Specialties.Select(s => s.SpecialtyName).ToList()))
                .ForMember(dest => dest.FeaturedImageUrl, opt => opt.MapFrom(src => src.Portfolios.OrderBy(p => p.DisplayOrder).FirstOrDefault()!.ImageUrl));

            CreateMap<Contractor, ContractorProfileDto>();
            CreateMap<ContractorSpecialty, ContractorSpecialtyDto>();
            CreateMap<ContractorDocument, ContractorDocumentDto>();
            CreateMap<ContractorPortfolio, ContractorPortfolioDto>();

            // CreateMap<Review, ReviewSummaryDto>()
            //     .ForMember(dest => dest.ReviewerName, opt => opt.MapFrom(src => src.Reviewer.Username))
            //     .ForMember(dest => dest.ProjectName, opt => opt.MapFrom(src => src.Project.ProjectName));
        }
    }
}
