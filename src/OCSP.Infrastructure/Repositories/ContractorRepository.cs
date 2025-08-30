using Microsoft.EntityFrameworkCore;
using OCSP.Domain.Entities;
using OCSP.Domain.Enums;
using OCSP.Infrastructure.Data;
using OCSP.Infrastructure.Repositories.Interfaces;

namespace OCSP.Infrastructure.Repositories
{
    public class ContractorRepository : GenericRepository<Contractor>, IContractorRepository
    {
        // Dùng _context protected từ GenericRepository<T>
        public ContractorRepository(ApplicationDbContext context) : base(context) { }

        public async Task<List<Contractor>> SearchContractorsAsync(
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
            int pageSize = 10)
        {
            var queryBuilder = _context.Contractors
                .Include(c => c.Specialties)
                .Include(c => c.Portfolios)
                .Where(c => c.IsActive && !c.IsRestricted);

            if (!string.IsNullOrEmpty(query))
            {
                queryBuilder = queryBuilder.Where(c =>
                    EF.Functions.ILike(c.CompanyName, $"%{query}%") ||
                    EF.Functions.ILike(c.Description ?? "", $"%{query}%") ||
                    c.Specialties.Any(s => EF.Functions.ILike(s.SpecialtyName, $"%{query}%")));
            }

            if (!string.IsNullOrEmpty(location))
            {
                queryBuilder = queryBuilder.Where(c =>
                    EF.Functions.ILike(c.City ?? "", $"%{location}%") ||
                    EF.Functions.ILike(c.Province ?? "", $"%{location}%"));
            }

            if (specialties?.Any() == true)
            {
                queryBuilder = queryBuilder.Where(c =>
                    c.Specialties.Any(s => specialties.Contains(s.SpecialtyName)));
            }

            if (minBudget.HasValue)
            {
                queryBuilder = queryBuilder.Where(c =>
                    !c.MaxProjectBudget.HasValue || c.MaxProjectBudget >= minBudget);
            }

            if (maxBudget.HasValue)
            {
                queryBuilder = queryBuilder.Where(c =>
                    !c.MinProjectBudget.HasValue || c.MinProjectBudget <= maxBudget);
            }

            if (minRating.HasValue)
            {
                queryBuilder = queryBuilder.Where(c => c.AverageRating >= minRating);
            }

            if (minYearsExperience.HasValue)
            {
                queryBuilder = queryBuilder.Where(c => c.YearsOfExperience >= minYearsExperience);
            }

            if (isVerified.HasValue)
            {
                queryBuilder = queryBuilder.Where(c => c.IsVerified == isVerified);
            }

            if (isPremium.HasValue)
            {
                queryBuilder = queryBuilder.Where(c => c.IsPremium == isPremium);
            }

            queryBuilder = sortBy switch
            {
                SearchSortBy.Rating => queryBuilder.OrderByDescending(c => c.AverageRating)
                                                   .ThenByDescending(c => c.TotalReviews),
                SearchSortBy.ExperienceYears => queryBuilder.OrderByDescending(c => c.YearsOfExperience),
                SearchSortBy.CompletedProjects => queryBuilder.OrderByDescending(c => c.CompletedProjects),
                SearchSortBy.PriceAsc => queryBuilder.OrderBy(c => c.MinProjectBudget ?? decimal.MaxValue),
                SearchSortBy.PriceDesc => queryBuilder.OrderByDescending(c => c.MaxProjectBudget ?? 0),
                SearchSortBy.Newest => queryBuilder.OrderByDescending(c => c.CreatedAt),
                SearchSortBy.Premium => queryBuilder.OrderByDescending(c => c.IsPremium)
                                                    .ThenByDescending(c => c.AverageRating),
                _ => queryBuilder.OrderByDescending(c => c.IsPremium)
                                 .ThenByDescending(c => c.AverageRating)
                                 .ThenByDescending(c => c.CompletedProjects)
            };

            return await queryBuilder
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetSearchCountAsync(
            string? query = null,
            string? location = null,
            List<string>? specialties = null,
            decimal? minBudget = null,
            decimal? maxBudget = null,
            decimal? minRating = null,
            int? minYearsExperience = null,
            bool? isVerified = null,
            bool? isPremium = null)
        {
            var queryBuilder = _context.Contractors
                .Where(c => c.IsActive && !c.IsRestricted);

            if (!string.IsNullOrEmpty(query))
            {
                queryBuilder = queryBuilder.Where(c =>
                    EF.Functions.ILike(c.CompanyName, $"%{query}%") ||
                    EF.Functions.ILike(c.Description ?? "", $"%{query}%") ||
                    c.Specialties.Any(s => EF.Functions.ILike(s.SpecialtyName, $"%{query}%")));
            }

            if (!string.IsNullOrEmpty(location))
            {
                queryBuilder = queryBuilder.Where(c =>
                    EF.Functions.ILike(c.City ?? "", $"%{location}%") ||
                    EF.Functions.ILike(c.Province ?? "", $"%{location}%"));
            }

            if (specialties?.Any() == true)
            {
                queryBuilder = queryBuilder.Where(c =>
                    c.Specialties.Any(s => specialties.Contains(s.SpecialtyName)));
            }

            if (minBudget.HasValue)
            {
                queryBuilder = queryBuilder.Where(c =>
                    !c.MaxProjectBudget.HasValue || c.MaxProjectBudget >= minBudget);
            }

            if (maxBudget.HasValue)
            {
                queryBuilder = queryBuilder.Where(c =>
                    !c.MinProjectBudget.HasValue || c.MinProjectBudget <= maxBudget);
            }

            if (minRating.HasValue)
            {
                queryBuilder = queryBuilder.Where(c => c.AverageRating >= minRating);
            }

            if (minYearsExperience.HasValue)
            {
                queryBuilder = queryBuilder.Where(c => c.YearsOfExperience >= minYearsExperience);
            }

            if (isVerified.HasValue)
            {
                queryBuilder = queryBuilder.Where(c => c.IsVerified == isVerified);
            }

            if (isPremium.HasValue)
            {
                queryBuilder = queryBuilder.Where(c => c.IsPremium == isPremium);
            }

            return await queryBuilder.CountAsync();
        }

        public async Task<Contractor?> GetByIdWithDetailsAsync(Guid id)
        {
            return await _context.Contractors
                .Include(c => c.User)
                .Include(c => c.Specialties)
                .Include(c => c.Documents)
                .Include(c => c.Portfolios.OrderBy(p => p.DisplayOrder))
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
        }

        public async Task<List<Review>> GetRecentReviewsAsync(Guid contractorId, int count = 5)
        {
            return await _context.Reviews
                .Include(r => r.Reviewer)
                .Include(r => r.Project)
                .Where(r => r.ContractorId == contractorId)
                .OrderByDescending(r => r.CreatedAt)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Contractor>> GetFeaturedContractorsAsync(int count = 10)
        {
            return await _context.Contractors
                .Include(c => c.Specialties)
                .Where(c => c.IsActive && !c.IsRestricted && c.IsVerified)
                .OrderByDescending(c => c.IsPremium)
                .ThenByDescending(c => c.AverageRating)
                .ThenByDescending(c => c.CompletedProjects)
                .Take(count)
                .ToListAsync();
        }

        public async Task<List<Contractor>> GetBySpecialtyAsync(string specialty)
        {
            return await _context.Contractors
                .Include(c => c.Specialties)
                .Where(c => c.IsActive && !c.IsRestricted &&
                            c.Specialties.Any(s => EF.Functions.ILike(s.SpecialtyName, $"%{specialty}%")))
                .OrderByDescending(c => c.AverageRating)
                .ToListAsync();
        }

        public async Task UpdateRatingAsync(Guid contractorId, decimal newRating, int newReviewCount)
        {
            var contractor = await GetByIdAsync(contractorId);
            if (contractor == null) return;

            contractor.AverageRating = newRating;
            contractor.TotalReviews = newReviewCount;

            await UpdateAsync(contractor);
        }
    }

    public class CommunicationRepository : GenericRepository<Communication>, ICommunicationRepository
    {
        // Dùng _context protected từ GenericRepository<T>
        public CommunicationRepository(ApplicationDbContext context) : base(context) { }

        public async Task<int> GetUserWarningCountAsync(Guid userId)
        {
            var contractor = await _context.Contractors.FirstOrDefaultAsync(c => c.UserId == userId);
            return contractor?.WarningCount ?? 0;
        }

        public async Task IncrementWarningCountAsync(Guid userId)
        {
            var contractor = await _context.Contractors.FirstOrDefaultAsync(c => c.UserId == userId);
            if (contractor != null)
            {
                contractor.WarningCount++;
                contractor.LastWarningDate = DateTime.UtcNow;

                if (contractor.WarningCount >= 3)
                {
                    contractor.IsRestricted = true;
                    contractor.RestrictionExpiryDate = DateTime.UtcNow.AddDays(30);
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Communication>> GetFlaggedCommunicationsAsync()
        {
            return await _context.Communications
                .Include(c => c.FromUser)
                .Include(c => c.ToUser)
                .Include(c => c.Project)
                .Where(c => c.IsFlagged && !c.IsReviewed)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Communication>> GetUserCommunicationsAsync(Guid userId, DateTime from, DateTime to)
        {
            return await _context.Communications
                .Where(c => (c.FromUserId == userId || c.ToUserId == userId) &&
                            c.CreatedAt >= from && c.CreatedAt <= to)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }
    }
}
