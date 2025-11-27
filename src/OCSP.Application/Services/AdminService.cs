using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OCSP.Application.Common.Exceptions;
using OCSP.Application.Common.Helpers;
using OCSP.Application.DTOs.Admin;
using OCSP.Application.DTOs.Auth;
using OCSP.Application.Services.Interfaces;
using OCSP.Domain.Entities;
using OCSP.Domain.Enums;
using OCSP.Infrastructure.Data;
using OCSP.Infrastructure.ExternalServices.Interfaces;

namespace OCSP.Application.Services
{
    public class AdminService : IAdminService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;

        public AdminService(
            ApplicationDbContext context,
            IMapper mapper,
            IEmailService emailService)
        {
            _context = context;
            _mapper = mapper;
            _emailService = emailService;
        }

        public async Task<UserResponseDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            // Validate email unique
            var existingUser = await _context.Users.FirstOrDefaultAsync(x => x.Email == createUserDto.Email);
            if (existingUser != null)
            {
                throw new ValidationException("Email đã được sử dụng");
            }

            // Validate username unique
            var existingUsername = await _context.Users.FirstOrDefaultAsync(x => x.Username == createUserDto.Username);
            if (existingUsername != null)
            {
                throw new ValidationException("Tên người dùng đã được sử dụng");
            }

            // Validate password
            if (string.IsNullOrWhiteSpace(createUserDto.Password) || createUserDto.Password.Length < 6)
            {
                throw new ValidationException("Mật khẩu phải có ít nhất 6 ký tự");
            }

            var verificationToken = PasswordHelper.GenerateRandomCode(6);

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = createUserDto.Username.Trim(),
                Email = createUserDto.Email.Trim().ToLowerInvariant(),
                PasswordHash = PasswordHelper.HashPassword(createUserDto.Password),
                Role = createUserDto.Role,
                IsEmailVerified = createUserDto.SkipEmailVerification, // Admin có thể bỏ qua verification
                EmailVerificationToken = createUserDto.SkipEmailVerification ? null : verificationToken,
                EmailVerificationTokenExpiry = createUserDto.SkipEmailVerification ? null : DateTime.UtcNow.AddDays(7), // Tăng thời gian cho admin tạo
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Gửi email verification nếu không skip
            if (!createUserDto.SkipEmailVerification)
            {
                try
                {
                    await _emailService.SendVerificationEmailAsync(user.Email, verificationToken);
                }
                catch (Exception ex)
                {
                    // Log error nhưng không fail request
                    Console.WriteLine($"Failed to send verification email: {ex.Message}");
                }
            }

            return _mapper.Map<UserResponseDto>(user);
        }

        public async Task<List<UserResponseDto>> GetAllUsersAsync()
        {
            var users = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            return _mapper.Map<List<UserResponseDto>>(users);
        }

        public async Task<List<AdminUserDto>> GetAllUsersWithProjectsAsync()
        {
            var users = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();

            var result = new List<AdminUserDto>();

            foreach (var user in users)
            {
                var userDto = new AdminUserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role,
                    IsEmailVerified = user.IsEmailVerified,
                    IsBanned = user.IsBanned,
                    CreatedAt = user.CreatedAt,
                    Projects = await GetUserProjectsAsync(user.Id)
                };

                result.Add(userDto);
            }

            return result;
        }

        private async Task<List<UserProjectInfoDto>> GetUserProjectsAsync(Guid userId)
        {
            var projects = new List<UserProjectInfoDto>();

            // 1. Lấy projects mà user là Homeowner
            var homeownerProjects = await _context.Projects
                .Where(p => p.HomeownerId == userId)
                .Select(p => new UserProjectInfoDto
                {
                    ProjectId = p.Id,
                    ProjectName = p.Name,
                    ProjectStatus = p.Status.ToString(),
                    ParticipationRole = "Homeowner",
                    JoinedAt = p.CreatedAt
                })
                .ToListAsync();

            projects.AddRange(homeownerProjects);

            // 2. Lấy projects mà user là Participant
            var participantProjects = await _context.ProjectParticipants
                .Include(pp => pp.Project)
                .Where(pp => pp.UserId == userId)
                .Select(pp => new UserProjectInfoDto
                {
                    ProjectId = pp.Project.Id,
                    ProjectName = pp.Project.Name,
                    ProjectStatus = pp.Project.Status.ToString(),
                    ParticipationRole = pp.Role.ToString(),
                    JoinedAt = pp.JoinedAt ?? pp.CreatedAt
                })
                .ToListAsync();

            projects.AddRange(participantProjects);

            // 3. Lấy projects mà user là Supervisor (qua Supervisor entity)
            var supervisorProjects = await (from p in _context.Projects
                                           join s in _context.Supervisors on p.SupervisorId equals s.Id into supervisorJoin
                                           from sup in supervisorJoin.DefaultIfEmpty()
                                           where sup != null && sup.UserId == userId
                                           select new UserProjectInfoDto
                                           {
                                               ProjectId = p.Id,
                                               ProjectName = p.Name,
                                               ProjectStatus = p.Status.ToString(),
                                               ParticipationRole = "Supervisor",
                                               JoinedAt = p.CreatedAt
                                           }).ToListAsync();

            projects.AddRange(supervisorProjects);

            // Loại bỏ duplicate và sắp xếp theo ngày tham gia
            return projects
                .GroupBy(p => p.ProjectId)
                .Select(g => g.First())
                .OrderByDescending(p => p.JoinedAt)
                .ToList();
        }

        public async Task<UserResponseDto?> GetUserByIdAsync(Guid userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return null;

            return _mapper.Map<UserResponseDto>(user);
        }

        public async Task<AdminUserDto?> GetUserByIdWithProjectsAsync(Guid userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return null;

            return new AdminUserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                IsEmailVerified = user.IsEmailVerified,
                IsBanned = user.IsBanned,
                CreatedAt = user.CreatedAt,
                Projects = await GetUserProjectsAsync(user.Id)
            };
        }

        public async Task<bool> DeleteUserAsync(Guid userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new ValidationException("Người dùng không tồn tại");
            }

            // Không cho phép xóa admin khác
            if (user.Role == OCSP.Domain.Enums.UserRole.Admin)
            {
                throw new ValidationException("Không thể xóa tài khoản admin");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<AdminDashboardStatsDto> GetDashboardStatsAsync()
        {
            // Basic counts
            var totalUsers = await _context.Users.CountAsync();
            var totalProjects = await _context.Projects.CountAsync();
            var totalProposals = await _context.Proposals.CountAsync();
            var totalQuoteRequests = await _context.QuoteRequests.CountAsync();
            var totalContracts = await _context.Contracts.CountAsync();

            // Project status breakdown
            var activeProjects = await _context.Projects
                .CountAsync(p => p.Status == ProjectStatus.Active);
            var completedProjects = await _context.Projects
                .CountAsync(p => p.Status == ProjectStatus.Completed);

            // Proposal status (assuming Proposal has Status field, if not, count all as pending)
            var pendingProposals = await _context.Proposals.CountAsync(); // Adjust based on actual Proposal status field

            // Contract status breakdown
            var activeContracts = await _context.Contracts
                .CountAsync(c => c.Status == ContractStatus.Active);
            var completedContracts = await _context.Contracts
                .CountAsync(c => c.Status == ContractStatus.Completed);

            // Total transaction value (sum of successful payments)
            var totalTransactionValue = await _context.PaymentTransactions
                .Where(pt => pt.Status == PaymentStatus.Succeeded)
                .SumAsync(pt => (decimal?)pt.Amount) ?? 0;

            // If no payment transactions, calculate from completed contracts
            if (totalTransactionValue == 0)
            {
                totalTransactionValue = await _context.Contracts
                    .Where(c => c.Status == ContractStatus.Completed || c.Status == ContractStatus.Active)
                    .SumAsync(c => (decimal?)c.TotalPrice) ?? 0;
            }

            // Calculate commission: CHỈ tính từ các giao dịch thanh toán phí môi giới
            // Phí môi giới là 0.1% giá trị hợp đồng, được chủ thầu thanh toán trước khi ký hợp đồng
            var commissionPayments = await _context.PaymentTransactions
                .Where(pt => pt.Status == PaymentStatus.Succeeded
                    && pt.Description != null 
                    && pt.Description.ToLower().Contains("phí môi giới"))
                .SumAsync(pt => (decimal?)pt.Amount) ?? 0;
            var totalCommission = commissionPayments;

            return new AdminDashboardStatsDto
            {
                TotalUsers = totalUsers,
                TotalProjects = totalProjects,
                TotalProposals = totalProposals,
                TotalQuoteRequests = totalQuoteRequests,
                TotalContracts = totalContracts,
                TotalTransactionValue = totalTransactionValue,
                TotalCommission = totalCommission,
                ActiveProjects = activeProjects,
                CompletedProjects = completedProjects,
                PendingProposals = pendingProposals,
                ActiveContracts = activeContracts,
                CompletedContracts = completedContracts
            };
        }

        public async Task<List<RecentProjectDto>> GetRecentProjectsAsync(int limit = 10)
        {
            var projects = await (from p in _context.Projects
                                 join h in _context.Users on p.HomeownerId equals h.Id
                                 join c in _context.Contractors on p.ContractorId equals c.Id into contractorJoin
                                 from contractor in contractorJoin.DefaultIfEmpty()
                                 join cu in _context.Users on (contractor != null ? contractor.UserId : (Guid?)null) equals cu.Id into contractorUserJoin
                                 from contractorUser in contractorUserJoin.DefaultIfEmpty()
                                 orderby p.CreatedAt descending
                                 select new RecentProjectDto
                                 {
                                     Id = p.Id,
                                     Name = p.Name,
                                     HomeownerName = h.Username,
                                     ContractorName = contractorUser != null ? contractorUser.Username : null,
                                     Budget = p.Budget,
                                     Status = p.Status.ToString(),
                                     CreatedAt = p.CreatedAt,
                                     UpdatedAt = p.UpdatedAt
                                 })
                                 .Take(limit)
                                 .ToListAsync();

            return projects;
        }

        public async Task<List<RecentUserDto>> GetRecentUsersAsync(int limit = 10)
        {
            var users = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(limit)
                .Select(u => new RecentUserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role,
                    IsEmailVerified = u.IsEmailVerified,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return users;
        }

        public async Task<List<AdminProjectListDto>> GetAllProjectsAsync(
            string? searchTerm = null,
            string? status = null,
            int page = 1,
            int pageSize = 20)
        {
            // Get projects with basic joins
            var query = from p in _context.Projects
                       join h in _context.Users on p.HomeownerId equals h.Id
                       join c in _context.Contractors on p.ContractorId equals c.Id into contractorJoin
                       from contractor in contractorJoin.DefaultIfEmpty()
                       join cu in _context.Users on (contractor != null ? contractor.UserId : (Guid?)null) equals cu.Id into contractorUserJoin
                       from contractorUser in contractorUserJoin.DefaultIfEmpty()
                       join s in _context.Supervisors on p.SupervisorId equals s.Id into supervisorJoin
                       from supervisor in supervisorJoin.DefaultIfEmpty()
                       join su in _context.Users on (supervisor != null ? supervisor.UserId : (Guid?)null) equals su.Id into supervisorUserJoin
                       from supervisorUser in supervisorUserJoin.DefaultIfEmpty()
                       select new { p, h, contractorUser, supervisorUser, contractor };

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(x => 
                    x.p.Name.Contains(searchTerm) ||
                    x.p.Description != null && x.p.Description.Contains(searchTerm) ||
                    x.p.Address.Contains(searchTerm) ||
                    x.h.Username.Contains(searchTerm) ||
                    (x.contractorUser != null && x.contractorUser.Username.Contains(searchTerm)) ||
                    (x.supervisorUser != null && x.supervisorUser.Username.Contains(searchTerm)));
            }

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(status))
            {
                if (Enum.TryParse<ProjectStatus>(status, out var statusEnum))
                {
                    query = query.Where(x => x.p.Status == statusEnum);
                }
            }

            // Order by created date descending
            query = query.OrderByDescending(x => x.p.CreatedAt);

            // Apply pagination and get project data
            var projectData = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new
                {
                    Project = x.p,
                    HomeownerName = x.h.Username,
                    ContractorUser = x.contractorUser,
                    SupervisorUser = x.supervisorUser
                })
                .ToListAsync();

            if (!projectData.Any())
            {
                return new List<AdminProjectListDto>();
            }

            // Get contractor from contracts for projects that don't have ContractorId set
            var projectIds = projectData.Select(x => x.Project.Id).ToList();
            Dictionary<Guid, string> contractDict = new Dictionary<Guid, string>();

            if (projectIds.Any())
            {
                // Get the latest contract for each project
                var contractsWithContractors = await _context.Contracts
                    .Where(c => projectIds.Contains(c.ProjectId) &&
                                (c.Status == ContractStatus.Completed || c.Status == ContractStatus.Active))
                    .GroupBy(c => c.ProjectId)
                    .Select(g => new
                    {
                        ProjectId = g.Key,
                        LatestContract = g.OrderByDescending(c => c.CreatedAt).First()
                    })
                    .ToListAsync();

                if (contractsWithContractors.Any())
                {
                    var contractorUserIds = contractsWithContractors
                        .Select(x => x.LatestContract.ContractorUserId)
                        .Distinct()
                        .ToList();

                    var contractorUsers = await _context.Users
                        .Where(u => contractorUserIds.Contains(u.Id))
                        .ToDictionaryAsync(u => u.Id, u => u.Username);

                    foreach (var item in contractsWithContractors)
                    {
                        if (contractorUsers.TryGetValue(item.LatestContract.ContractorUserId, out var username))
                        {
                            contractDict[item.ProjectId] = username;
                        }
                    }
                }
            }

            // Build result
            var result = projectData.Select(x =>
            {
                string? contractorName = null;
                
                // First try from Project.ContractorId (via ContractorUser)
                if (x.ContractorUser != null)
                {
                    contractorName = x.ContractorUser.Username;
                }
                // If not available, try from Contract
                else if (contractDict.TryGetValue(x.Project.Id, out var contractContractor))
                {
                    contractorName = contractContractor;
                }

                return new AdminProjectListDto
                {
                    Id = x.Project.Id,
                    Name = x.Project.Name,
                    Description = x.Project.Description,
                    Address = x.Project.Address,
                    HomeownerName = x.HomeownerName,
                    ContractorName = contractorName,
                    SupervisorName = x.SupervisorUser != null ? x.SupervisorUser.Username : null,
                    Budget = x.Project.Budget,
                    ActualBudget = x.Project.ActualBudget,
                    Status = x.Project.Status.ToString(),
                    StartDate = x.Project.StartDate,
                    EndDate = x.Project.EndDate,
                    CreatedAt = x.Project.CreatedAt,
                    UpdatedAt = x.Project.UpdatedAt
                };
            }).ToList();

            return result;
        }

        public async Task<FinancialReportDto> GetFinancialReportAsync()
        {
            var now = DateTime.UtcNow;
            var twelveMonthsAgo = now.AddMonths(-12);

            // Get all successful payment transactions
            var successfulPayments = await _context.PaymentTransactions
                .Where(pt => pt.Status == PaymentStatus.Succeeded)
                .ToListAsync();

            // Get all completed contracts
            var completedContracts = await _context.Contracts
                .Where(c => c.Status == ContractStatus.Completed)
                .ToListAsync();

            var activeContracts = await _context.Contracts
                .Where(c => c.Status == ContractStatus.Active)
                .ToListAsync();

            // Calculate totals
            var totalRevenue = successfulPayments.Sum(pt => pt.Amount);
            var completedContractValue = completedContracts.Sum(c => c.TotalPrice);
            var activeContractValue = activeContracts.Sum(c => c.TotalPrice);

            // Calculate commission: CHỈ tính từ các giao dịch thanh toán phí môi giới
            // Phí môi giới là 0.1% giá trị hợp đồng, được chủ thầu thanh toán trước khi ký hợp đồng
            // PaymentTransaction có Description = "Phí môi giới" chính là tiền hoa hồng
            var commissionPayments = successfulPayments
                .Where(pt => pt.Description != null && pt.Description.ToLower().Contains("phí môi giới"))
                .ToList();
            var totalCommission = commissionPayments.Sum(pt => pt.Amount);

            // Calculate expenses (assume 10% of revenue for platform costs)
            const decimal expenseRate = 0.10m;
            var totalExpenses = totalRevenue * expenseRate;
            var netProfit = totalCommission - totalExpenses;

            // Get transaction statistics
            var totalTransactions = await _context.PaymentTransactions.CountAsync();
            var successfulTransactions = await _context.PaymentTransactions
                .CountAsync(pt => pt.Status == PaymentStatus.Succeeded);
            var failedTransactions = await _context.PaymentTransactions
                .CountAsync(pt => pt.Status == PaymentStatus.Failed);
            var pendingTransactions = await _context.PaymentTransactions
                .CountAsync(pt => pt.Status == PaymentStatus.Pending);

            // Calculate pending payment value
            var pendingPayments = await _context.PaymentTransactions
                .Where(pt => pt.Status == PaymentStatus.Pending)
                .SumAsync(pt => (decimal?)pt.Amount) ?? 0;

            // Get detailed statistics for explanation
            var totalProjects = await _context.Projects.CountAsync();
            var totalContracts = await _context.Contracts.CountAsync();
            var totalSuccessfulPaymentTransactions = successfulTransactions;
            
            // Calculate transaction amount statistics
            var transactionAmounts = successfulPayments.Select(pt => pt.Amount).ToList();
            var averageTransactionAmount = transactionAmounts.Any() ? transactionAmounts.Average() : 0;
            var largestTransactionAmount = transactionAmounts.Any() ? transactionAmounts.Max() : 0;
            var smallestTransactionAmount = transactionAmounts.Any() ? transactionAmounts.Min() : 0;

            // Get monthly data (last 12 months)
            var monthlyData = new List<MonthlyFinancialDto>();
            for (int i = 11; i >= 0; i--)
            {
                var monthDate = now.AddMonths(-i);
                var monthStart = new DateTime(monthDate.Year, monthDate.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                var monthEnd = monthStart.AddMonths(1).AddTicks(-1);

                // Query monthly revenue from database directly for accuracy
                var monthRevenue = await _context.PaymentTransactions
                    .Where(pt => pt.Status == PaymentStatus.Succeeded 
                        && pt.CreatedAt >= monthStart 
                        && pt.CreatedAt <= monthEnd)
                    .SumAsync(pt => (decimal?)pt.Amount) ?? 0;

                // Monthly commission: chỉ tính từ các giao dịch phí môi giới trong tháng
                var monthCommissionPayments = await _context.PaymentTransactions
                    .Where(pt => pt.Status == PaymentStatus.Succeeded 
                        && pt.CreatedAt >= monthStart 
                        && pt.CreatedAt <= monthEnd
                        && pt.Description != null 
                        && pt.Description.ToLower().Contains("phí môi giới"))
                    .SumAsync(pt => (decimal?)pt.Amount) ?? 0;
                var monthCommission = monthCommissionPayments;
                var monthExpenses = monthRevenue * expenseRate;
                var monthTransactionCount = await _context.PaymentTransactions
                    .CountAsync(pt => pt.CreatedAt >= monthStart 
                        && pt.CreatedAt <= monthEnd 
                        && pt.Status == PaymentStatus.Succeeded);

                monthlyData.Add(new MonthlyFinancialDto
                {
                    Month = monthStart.ToString("yyyy-MM"),
                    MonthName = $"Tháng {monthStart:MM/yyyy}",
                    Revenue = monthRevenue,
                    Expenses = monthExpenses,
                    Commission = monthCommission,
                    TransactionCount = monthTransactionCount
                });
            }

            return new FinancialReportDto
            {
                TotalRevenue = totalRevenue,
                TotalExpenses = totalExpenses,
                NetProfit = netProfit,
                TotalCommission = totalCommission,
                MonthlyData = monthlyData,
                CompletedContractValue = completedContractValue,
                ActiveContractValue = activeContractValue,
                PendingPaymentValue = pendingPayments,
                TotalTransactions = totalTransactions,
                SuccessfulTransactions = successfulTransactions,
                FailedTransactions = failedTransactions,
                PendingTransactions = pendingTransactions,
                // Chi tiết để giải thích
                TotalProjects = totalProjects,
                TotalContracts = totalContracts,
                TotalSuccessfulPaymentTransactions = totalSuccessfulPaymentTransactions,
                AverageTransactionAmount = averageTransactionAmount,
                LargestTransactionAmount = largestTransactionAmount,
                SmallestTransactionAmount = smallestTransactionAmount
            };
        }

        public async Task<bool> BanUserAsync(Guid userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new ValidationException("Người dùng không tồn tại");
            }

            // Không cho phép ban admin
            if (user.Role == OCSP.Domain.Enums.UserRole.Admin)
            {
                throw new ValidationException("Không thể ban tài khoản admin");
            }

            try
            {
                user.IsBanned = true;
                user.UpdatedAt = DateTime.UtcNow;
                // Vô hiệu hóa refresh token khi ban
                user.RefreshToken = null;
                user.RefreshTokenExpiryTime = null;

                var rowsAffected = await _context.SaveChangesAsync();
                if (rowsAffected == 0)
                {
                    throw new ValidationException("Không thể cập nhật trạng thái ban của người dùng");
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new ValidationException($"Lỗi khi ban người dùng: {ex.Message}");
            }
        }

        public async Task<bool> UnbanUserAsync(Guid userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new ValidationException("Người dùng không tồn tại");
            }

            // Không cho phép unban admin (vì admin không thể bị ban)
            if (user.Role == OCSP.Domain.Enums.UserRole.Admin)
            {
                throw new ValidationException("Không thể unban tài khoản admin");
            }

            try
            {
                user.IsBanned = false;
                user.UpdatedAt = DateTime.UtcNow;

                var rowsAffected = await _context.SaveChangesAsync();
                if (rowsAffected == 0)
                {
                    throw new ValidationException("Không thể cập nhật trạng thái unban của người dùng");
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new ValidationException($"Lỗi khi unban người dùng: {ex.Message}");
            }
        }
    }
}
