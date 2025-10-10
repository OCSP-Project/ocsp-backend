using OCSP.Application.DTOs.Project;
using OCSP.Application.Services.Interfaces;
using OCSP.Infrastructure.Repositories.Interfaces;
using OCSP.Domain.Entities;
using Microsoft.AspNetCore.Http;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUserRepository _userRepository;
<<<<<<< HEAD
    private readonly IProjectDocumentService _documentService;
=======
    private readonly IContractorRepository _contractorRepository;

>>>>>>> 534010e58df290d50acc383510252452373b6c4c

    public ProjectService(
        IProjectRepository projectRepository,
        IUserRepository userRepository,
<<<<<<< HEAD
        IProjectDocumentService documentService)
    {
        _projectRepository = projectRepository;
        _userRepository = userRepository;
        _documentService = documentService;
=======
        IContractorRepository contractorRepository)
    {
        _projectRepository = projectRepository;
        _userRepository = userRepository;
        _contractorRepository = contractorRepository;
>>>>>>> 534010e58df290d50acc383510252452373b6c4c
    }

    public async Task<List<ProjectResponseDto>> GetProjectsByHomeownerAsync(Guid homeownerId, CancellationToken ct = default)
    {
        var homeowner = await _userRepository.GetByIdAsync(homeownerId);
        if (homeowner == null)
            throw new ArgumentException("Homeowner not found");

        var projects = await _projectRepository.GetByHomeownerIdAsync(homeownerId, ct);

        var result = projects.Select(p => new ProjectResponseDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Address = p.Address,
            FloorArea = p.FloorArea,
            NumberOfFloors = p.NumberOfFloors,
            Budget = p.Budget,
            ActualBudget = p.ActualBudget,
            StartDate = p.StartDate,
            EndDate = p.EndDate,
            EstimatedCompletionDate = p.EstimatedCompletionDate,
            Status = p.Status.ToString(),
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
            SupervisorId = p.SupervisorId,
            SupervisorName = p.Supervisor?.User?.Username,
            HomeownerId = p.HomeownerId,
            HomeownerName = p.Homeowner?.Username
        }).ToList();

        return result;
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
                    UserName = pp.User?.Username ?? "Unknown User",
                    Role = pp.Role.ToString(),
                    Status = pp.Status.ToString()
                }).ToList()
        };
    }


    public async Task<ProjectDetailDto> CreateProjectWithFilesAsync(CreateProjectDto dto, IFormFile drawingFile, IFormFile permitFile, Guid homeownerId)
    {
        if (string.IsNullOrWhiteSpace(dto.Name)) throw new ArgumentException("Tên dự án là bắt buộc");
        if (string.IsNullOrWhiteSpace(dto.Address)) throw new ArgumentException("Địa chỉ là bắt buộc");
        if (dto.Budget <= 0) throw new ArgumentException("Ngân sách phải lớn hơn 0");

        if (drawingFile == null || drawingFile.Length == 0)
            throw new ArgumentException("Bản vẽ là bắt buộc");
        if (permitFile == null || permitFile.Length == 0)
            throw new ArgumentException("Giấy phép là bắt buộc");

        var drawingExt = Path.GetExtension(drawingFile.FileName).ToLowerInvariant();
        if (drawingExt != ".pdf")
            throw new ArgumentException("Bản vẽ chỉ chấp nhận file PDF");
        if (drawingFile.Length > 100 * 1024 * 1024)
            throw new ArgumentException("Bản vẽ quá lớn (tối đa 100MB)");

        _ = await _userRepository.GetByIdAsync(homeownerId)
            ?? throw new ArgumentException("Homeowner not found");

<<<<<<< HEAD
        // ✅ SỬ DỤNG DỮ LIỆU OCR TỪ FRONTEND THAY VÌ SCAN LẠI
        if (dto.FloorArea <= 0)
            throw new ArgumentException("Diện tích phải lớn hơn 0");
        if (dto.NumberOfFloors <= 0)
            throw new ArgumentException("Số tầng phải lớn hơn 0");

=======
        // Khởi tạo project (có thể gán Contractor)
>>>>>>> 534010e58df290d50acc383510252452373b6c4c
        var project = new Project
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim() ?? $"{dto.BuildingType ?? "Nhà"} - {dto.Address}",
            Address = dto.Address.Trim(),
            FloorArea = dto.FloorArea, // ✅ Sử dụng từ frontend
            NumberOfFloors = dto.NumberOfFloors, // ✅ Sử dụng từ frontend
            Budget = dto.Budget,
            StartDate = DateTime.UtcNow,
            EstimatedCompletionDate = null,
            Status = ProjectStatus.Active,
            HomeownerId = homeownerId,
            SupervisorId = null,
            ContractorId = dto.ContractorId // Gán contractor nếu có
        };

        // Thêm participant: Homeowner
        var participants = new List<ProjectParticipant>
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

        // Thêm Contractor vào participants nếu có
        if (dto.ContractorId.HasValue)
        {
            // Lấy UserId từ ContractorId
            var contractor = await _contractorRepository.GetByIdAsync(dto.ContractorId.Value);
            if (contractor != null)
            {
                participants.Add(new ProjectParticipant
                {
                    Project = project,
                    UserId = contractor.UserId, // Dùng UserId thay vì ContractorId
                    Role = ProjectRole.Contractor,
                    Status = ParticipantStatus.Active,
                    JoinedAt = DateTime.UtcNow
                });
            }
        }

        project.Participants = participants;

        await _projectRepository.AddAsync(project);
        await _projectRepository.SaveChangesAsync();

        try
        {
            await _documentService.UploadDocumentAsync(
                project.Id,
                homeownerId,
                drawingFile,
                new UploadProjectDocumentDto
                {
                    DocumentType = 1,
                    Description = "Bản vẽ thi công"
                }
            );

            await _documentService.UploadDocumentAsync(
                project.Id,
                homeownerId,
                permitFile,
                new UploadProjectDocumentDto
                {
                    DocumentType = 2,
                    Description = $"Giấy phép {dto.PermitNumber ?? "N/A"}"
                }
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to upload documents: {ex.Message}");
        }

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
            SupervisorId = project.SupervisorId,
            Participants = project.Participants.Select(pp => new ProjectParticipantDto
            {
                UserId = pp.UserId,
                UserName = pp.User?.Username ?? "Unknown User",
                Role = pp.Role.ToString(),
                Status = pp.Status.ToString()
            }).ToList()
        };
    }
    public async Task<ProjectDetailDto> UpdateProjectAsync(Guid projectId, UpdateProjectDto dto, Guid homeownerId)
    {
        var project = await _projectRepository.GetByIdAsync(projectId)
            ?? throw new ArgumentException("Project not found");

        if (project.HomeownerId != homeownerId)
            throw new UnauthorizedAccessException("You are not allowed to update this project");

        // Update từng property nếu khác null
        if (!string.IsNullOrWhiteSpace(dto.Name))
            project.Name = dto.Name.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Address))
            project.Address = dto.Address.Trim();
        if (dto.Description != null)
            project.Description = dto.Description.Trim();
        if (dto.FloorArea.HasValue)
            project.FloorArea = dto.FloorArea.Value;
        if (dto.NumberOfFloors.HasValue)
            project.NumberOfFloors = dto.NumberOfFloors.Value;
        if (dto.Budget.HasValue)
        {
            if (dto.Budget < 0) throw new ArgumentException("Budget must be >= 0");
            project.Budget = dto.Budget.Value;
        }
        if (dto.StartDate.HasValue)
            project.StartDate = dto.StartDate.Value;
        if (dto.EstimatedCompletionDate.HasValue)
        {
            if (dto.StartDate.HasValue && dto.EstimatedCompletionDate < dto.StartDate)
                throw new ArgumentException("EstimatedCompletionDate cannot be earlier than StartDate");
            project.EstimatedCompletionDate = dto.EstimatedCompletionDate;
        }
        if (!string.IsNullOrWhiteSpace(dto.Status))
        {
            if (Enum.TryParse<ProjectStatus>(dto.Status, true, out var status))
                project.Status = status;
            else
                throw new ArgumentException($"Invalid status: {dto.Status}. Valid values are: {string.Join(", ", Enum.GetNames<ProjectStatus>())}");
        }

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
            SupervisorId = project.SupervisorId,
            Participants = project.Participants.Select(pp => new ProjectParticipantDto
            {
                UserId = pp.UserId,
                UserName = pp.User?.Username ?? "Unknown User",
                Role = pp.Role.ToString(),
                Status = pp.Status.ToString()
            }).ToList()
        };
    }

}
