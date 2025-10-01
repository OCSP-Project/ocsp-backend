using System.Linq;
using AutoMapper;

using OCSP.Application.DTOs.Contractor;
using OCSP.Domain.Entities;

namespace OCSP.Application.Mappings
{
    public class ContractorMappingProfile : AutoMapper.Profile
    {
        public ContractorMappingProfile()
        {
            CreateMap<Contractor, ContractorProfileSummaryDto>()
                .ForMember(d => d.Specialties, o => o.MapFrom(s => s.Specialties.Select(x => x.SpecialtyName)))
                .ForMember(d => d.FeaturedImageUrl, o => o.MapFrom(s =>
                    s.Portfolios.OrderBy(p => p.DisplayOrder).Select(p => p.ImageUrl).FirstOrDefault()));

            CreateMap<Contractor, ContractorProfileDto>()
                .ForMember(d => d.OwnerUserId, o => o.MapFrom(s => (Guid?)s.UserId));
            CreateMap<ContractorSpecialty, ContractorSpecialtyDto>();
            CreateMap<ContractorDocument, ContractorDocumentDto>();
            CreateMap<ContractorPortfolio, ContractorPortfolioDto>();


            CreateMap<Contractor, ContractorSummaryDto>()
                .ForMember(d => d.Specialties, o => o.MapFrom(s => s.Specialties.Select(x => x.SpecialtyName)))
                .ForMember(d => d.FeaturedImageUrl, o => o.MapFrom(s =>
                    s.Portfolios.OrderBy(p => p.DisplayOrder).Select(p => p.ImageUrl).FirstOrDefault()))
                .ForMember(d => d.AverageRating, o => o.MapFrom(s => s.AverageRating))
                .ForMember(d => d.TotalReviews, o => o.MapFrom(s => s.TotalReviews))
                .ForMember(d => d.CompletedProjects, o => o.MapFrom(s => s.CompletedProjects))
                .ForMember(d => d.IsVerified, o => o.MapFrom(s => s.IsVerified))
                .ForMember(d => d.IsPremium, o => o.MapFrom(s => s.IsPremium));
        }
    }
}
