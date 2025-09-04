using Microsoft.EntityFrameworkCore;
using OCSP.Domain.Entities;
using OCSP.Domain.Enums;
using OCSP.Infrastructure.Data;

namespace OCSP.Infrastructure.Repositories.Interfaces
{

    public interface IContractorRepository : IGenericRepository<Contractor>
    {
        Task<List<Contractor>> SearchContractorsAsync(
            string? query = null,
            string? location = null,
            List<string>? specialties = null,
            decimal? minBudget = null,
            decimal? maxBudget = null,
            decimal? minRating = null,
            int? minYearsExperience = null,
            bool? isVerified = null,
            bool? isPremium = null,
            SearchSortBy sortBy = SearchSortBy.Relevance,
            int page = 1,
            int pageSize = 10);

        Task<int> GetSearchCountAsync(
            string? query = null,
            string? location = null,
            List<string>? specialties = null,
            decimal? minBudget = null,
            decimal? maxBudget = null,
            decimal? minRating = null,
            int? minYearsExperience = null,
            bool? isVerified = null,
            bool? isPremium = null);

        Task<Contractor?> GetByIdWithDetailsAsync(Guid id);
        Task<List<Review>> GetRecentReviewsAsync(Guid contractorId, int count = 5);
        Task<List<Contractor>> GetFeaturedContractorsAsync(int count = 10);
        Task<List<Contractor>> GetBySpecialtyAsync(string specialty);
        Task UpdateRatingAsync(Guid contractorId, decimal newRating, int newReviewCount);
    }

    public interface ICommunicationRepository : IGenericRepository<Communication>
    {
        Task<int> GetUserWarningCountAsync(Guid userId);
        Task IncrementWarningCountAsync(Guid userId);
        Task<List<Communication>> GetFlaggedCommunicationsAsync();
        Task<List<Communication>> GetUserCommunicationsAsync(Guid userId, DateTime from, DateTime to);
    }}