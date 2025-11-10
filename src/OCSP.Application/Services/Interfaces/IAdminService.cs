using OCSP.Application.DTOs.Admin;
using OCSP.Application.DTOs.Auth;

namespace OCSP.Application.Services.Interfaces
{
    public interface IAdminService
    {
        Task<UserResponseDto> CreateUserAsync(CreateUserDto createUserDto);
        Task<List<UserResponseDto>> GetAllUsersAsync();
        Task<List<AdminUserDto>> GetAllUsersWithProjectsAsync();
        Task<UserResponseDto?> GetUserByIdAsync(Guid userId);
        Task<AdminUserDto?> GetUserByIdWithProjectsAsync(Guid userId);
        Task<bool> DeleteUserAsync(Guid userId);
        Task<AdminDashboardStatsDto> GetDashboardStatsAsync();
        Task<List<RecentProjectDto>> GetRecentProjectsAsync(int limit = 10);
        Task<List<RecentUserDto>> GetRecentUsersAsync(int limit = 10);
        Task<List<AdminProjectListDto>> GetAllProjectsAsync(
            string? searchTerm = null,
            string? status = null,
            int page = 1,
            int pageSize = 20);
        Task<FinancialReportDto> GetFinancialReportAsync();
    }
}
