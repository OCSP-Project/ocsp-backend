using Microsoft.Extensions.Logging;
using OCSP.Application.DTOs.ModelAnalysis;
using OCSP.Application.Services.Interfaces;
using OCSP.Infrastructure.Services;
using OCSP.Domain.Entities;
using OCSP.Infrastructure.Repositories.Interfaces;
using System.Text.Json;

namespace OCSP.Application.Services
{
    public interface IModelAnalysisService
    {
        Task<UploadGLBResponse> UploadGLBAsync(
            UploadGLBRequest request,
            Guid userId,
            CancellationToken cancellationToken = default);

        Task<Project3DModelDto?> GetProjectModelAsync(
            Guid projectId,
            CancellationToken cancellationToken = default);

        Task<List<Project3DModelDto>> ListProjectModelsAsync(
            Guid projectId,
            CancellationToken cancellationToken = default);

        Task<Project3DModelDto?> GetModelByIdAsync(
            Guid modelId,
            CancellationToken cancellationToken = default);
    }

    public class ModelAnalysisService : IModelAnalysisService
    {
        private readonly IProject3DModelRepository _modelRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IFileStorageService _fileStorage;
        private readonly IGLBValidatorService _validator;
        private readonly ILogger<ModelAnalysisService> _logger;

        public ModelAnalysisService(
            IProject3DModelRepository modelRepository,
            IProjectRepository projectRepository,
            IFileStorageService fileStorage,
            IGLBValidatorService validator,
            ILogger<ModelAnalysisService> logger)
        {
            _modelRepository = modelRepository;
            _projectRepository = projectRepository;
            _fileStorage = fileStorage;
            _validator = validator;
            _logger = logger;
        }

        public async Task<UploadGLBResponse> UploadGLBAsync(UploadGLBRequest request, Guid userId, CancellationToken cancellationToken = default)
        {
            var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);
            if (project == null)
            {
                throw new InvalidOperationException($"Project {request.ProjectId} not found");
            }

            var validation = await _validator.ValidateGLBFileAsync(request.File, cancellationToken);
            if (!validation.IsValid)
            {
                return new UploadGLBResponse
                {
                    IsValid = false,
                    ValidationMessage = validation.ErrorMessage,
                    FileSizeMB = validation.FileSizeMB
                };
            }

            var fileName = $"{request.ProjectId}_{DateTime.UtcNow:yyyyMMddHHmmss}.glb";
            var fileUrl = await _fileStorage.UploadFileAsync(request.File, "3d-models", fileName, cancellationToken);

            var model = new Project3DModel
            {
                Id = Guid.NewGuid(),
                ProjectId = request.ProjectId,
                FileName = request.File.FileName,
                FileUrl = fileUrl,
                FileSizeMB = validation.FileSizeMB,
                TotalMeshes = validation.MeshCount,
                AnalysisCompleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = userId.ToString()
            };

            await _modelRepository.CreateAsync(model, cancellationToken);

            return new UploadGLBResponse
            {
                ModelId = model.Id,
                ProjectId = model.ProjectId,
                FileName = model.FileName,
                FileUrl = model.FileUrl,
                FileSizeMB = model.FileSizeMB,
                TotalMeshes = model.TotalMeshes,
                IsValid = true,
                ValidationMessage = "File uploaded successfully",
                AnalysisCompleted = false,
                UploadedAt = model.CreatedAt
            };
        }

        public async Task<Project3DModelDto?> GetProjectModelAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            var model = await _modelRepository.GetLatestByProjectIdAsync(projectId, cancellationToken);
            if (model == null)
            {
                return null;
            }

            return MapToDto(model);
        }

        public async Task<List<Project3DModelDto>> ListProjectModelsAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            var models = await _modelRepository.ListByProjectIdAsync(projectId, cancellationToken);
            return models.Select(MapToDto).ToList();
        }

        public async Task<Project3DModelDto?> GetModelByIdAsync(Guid modelId, CancellationToken cancellationToken = default)
        {
            var model = await _modelRepository.GetByIdAsync(modelId, cancellationToken);
            return model == null ? null : MapToDto(model);
        }

        private static Project3DModelDto MapToDto(Project3DModel model)
        {
            ModelAnalysisResult? analysisResult = null;
            if (!string.IsNullOrEmpty(model.AnalysisResultJson))
            {
                try
                {
                    analysisResult = JsonSerializer.Deserialize<ModelAnalysisResult>(model.AnalysisResultJson);
                }
                catch { }
            }

            return new Project3DModelDto
            {
                Id = model.Id,
                ProjectId = model.ProjectId,
                ProjectName = model.Project?.Name ?? string.Empty,
                FileName = model.FileName,
                FileUrl = model.FileUrl,
                FileSizeMB = model.FileSizeMB,
                TotalMeshes = model.TotalMeshes,
                AnalysisCompleted = model.AnalysisCompleted,
                AnalyzedAt = model.AnalyzedAt,
                AnalysisResult = analysisResult,
                CreatedAt = model.CreatedAt,
                CreatedBy = model.CreatedBy
            };
        }
    }
}
