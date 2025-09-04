using OCSP.Application.DTOs.Project;
using OCSP.Application.Services.Interfaces;
using OCSP.Infrastructure.Repositories.Interfaces;
using OCSP.Domain.Entities;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUserRepository _userRepository;

    public ProjectService(
        IProjectRepository projectRepository,
        IUserRepository userRepository)
    {
        _projectRepository = projectRepository;
        _userRepository = userRepository;
    }

    public async Task<ProjectDetailDto?> GetProjectByIdAsync(Guid id, CancellationToken ct = default)
    {
        var p = await _projectRepository.GetByIdAsync(id, ct);
        if (p == null) return null;

        return new ProjectDetailDto
        {
            Id = p.Id,
            Name = p.Name,
            Address = p.Address,
            Description = p.Description,
            FloorArea = p.FloorArea,
            NumberOfFloors = p.NumberOfFloors,
            Budget = p.Budget,
            StartDate = p.StartDate,
            EstimatedCompletionDate = p.EstimatedCompletionDate,
            Status = p.Status.ToString(),
            HomeownerId = p.HomeownerId,
            SupervisorId = p.SupervisorId, // sẽ là null cho tới khi bạn gán sau này
            Participants = (p.Participants ?? new List<ProjectParticipant>())
                .Select(pp => new ProjectParticipantDto
                {
                    UserId = pp.UserId,
                    Role = pp.Role.ToString(),
                    Status = pp.Status.ToString()
                }).ToList()
        };
    }

    public async Task<ProjectDetailDto> CreateProjectAsync(CreateProjectDto dto, Guid homeownerId)
    {
        // Validate cơ bản
        if (string.IsNullOrWhiteSpace(dto.Name)) throw new ArgumentException("Project name is required");
        if (string.IsNullOrWhiteSpace(dto.Address)) throw new ArgumentException("Project address is required");
        if (dto.Budget < 0) throw new ArgumentException("Budget must be >= 0");
        if (dto.StartDate == default) throw new ArgumentException("StartDate is required");
        if (dto.EstimatedCompletionDate.HasValue && dto.EstimatedCompletionDate < dto.StartDate)
            throw new ArgumentException("EstimatedCompletionDate cannot be earlier than StartDate");

        // Đảm bảo homeowner tồn tại
        _ = await _userRepository.GetByIdAsync(homeownerId)
            ?? throw new ArgumentException("Homeowner not found");

        // Khởi tạo project (KHÔNG gán Supervisor)
        var project = new Project
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim() ?? string.Empty,
            Address = dto.Address.Trim(),
            FloorArea = dto.FloorArea,
            NumberOfFloors = dto.NumberOfFloors,
            Budget = dto.Budget,
            StartDate = dto.StartDate,
            EstimatedCompletionDate = dto.EstimatedCompletionDate,
            Status = ProjectStatus.Active, // hoặc Draft tùy policy
            HomeownerId = homeownerId,
            SupervisorId = null
        };

        // Thêm participant: Homeowner
        project.Participants = new List<ProjectParticipant>
        {
            new ProjectParticipant
            {
                Project = project,
                UserId = homeownerId,
                Role = ProjectRole.Homeowner,
                Status = ParticipantStatus.Active,
                JoinedAt = DateTime.UtcNow
            }
        };

        await _projectRepository.AddAsync(project);
        await _projectRepository.SaveChangesAsync();

        // Map trả về
        return new ProjectDetailDto
        {
            Id = project.Id,
            Name = project.Name,
            Address = project.Address,
            Description = project.Description,
            FloorArea = project.FloorArea,
            NumberOfFloors = project.NumberOfFloors,
            Budget = project.Budget,
            StartDate = project.StartDate,
            EstimatedCompletionDate = project.EstimatedCompletionDate,
            Status = project.Status.ToString(),
            HomeownerId = project.HomeownerId,
            SupervisorId = project.SupervisorId, // null
            Participants = project.Participants.Select(pp => new ProjectParticipantDto
            {
                UserId = pp.UserId,
                Role = pp.Role.ToString(),
                Status = pp.Status.ToString()
            }).ToList()
        };
    }
}
