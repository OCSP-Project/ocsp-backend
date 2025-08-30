using OCSP.Application.DTOs.Project;
using OCSP.Application.Services.Interfaces;
using OCSP.Domain.Entities;
using OCSP.Infrastructure.Repositories.Interfaces;
using OCSP.Domain.Common;

namespace OCSP.Application.Services
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IUserRepository _userRepository;

        public ProjectService(IProjectRepository projectRepository, IUserRepository userRepository)
        {
            _projectRepository = projectRepository;
            _userRepository = userRepository;
        }

        // Create Project
        public async Task<ProjectResponseDto> CreateProjectAsync(CreateProjectDto createDto, Guid homeownerId)
        {
            // Validate homeowner exists
            var homeowner = await _userRepository.GetByIdAsync(homeownerId);
            if (homeowner == null)
                throw new ArgumentException("Homeowner not found");

            // Create new project
            var project = new Project
            {
                Name = createDto.Name,
                Description = createDto.Description,
                Address = createDto.Address,
                FloorArea = createDto.FloorArea,
                NumberOfFloors = createDto.NumberOfFloors,
                Budget = createDto.Budget,
                StartDate = createDto.StartDate,
                EndDate = createDto.EndDate,
                EstimatedCompletionDate = createDto.EstimatedCompletionDate,
                Status = createDto.Status,
                HomeownerId = homeownerId, // Set the homeowner ID
                SupervisorId = null, 
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdProject = await _projectRepository.AddAsync(project);
            await _projectRepository.SaveChangesAsync();

            return new ProjectResponseDto(
                createdProject.Id,
                createdProject.Name,
                createdProject.Description,
                createdProject.Address,
                createdProject.FloorArea,
                createdProject.NumberOfFloors,
                createdProject.Budget,
                createdProject.ActualBudget,
                createdProject.StartDate,
                createdProject.EndDate,
                createdProject.EstimatedCompletionDate,
                createdProject.Status,
                createdProject.CreatedAt,
                createdProject.UpdatedAt,
                createdProject.SupervisorId,
                createdProject.Supervisor?.User?.Username,
                createdProject.HomeownerId,
                createdProject.Homeowner?.Username
            );
        }

    }
}