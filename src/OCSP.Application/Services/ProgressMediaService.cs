using AutoMapper;
using OCSP.Application.DTOs.ProgressMedia;
using OCSP.Application.Services.Interfaces;
using OCSP.Domain.Entities;
using OCSP.Infrastructure.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;

namespace OCSP.Application.Services
{
    public class ProgressMediaService : IProgressMediaService
    {
        private readonly IProgressMediaRepository _progressMediaRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IFileService _fileService;
        private readonly IMapper _mapper;

        public ProgressMediaService(
            IProgressMediaRepository progressMediaRepository,
            IProjectRepository projectRepository,
            IFileService fileService,
            IMapper mapper)
        {
            _progressMediaRepository = progressMediaRepository;
            _projectRepository = projectRepository;
            _fileService = fileService;
            _mapper = mapper;
        }

        public async Task<ProgressMediaDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var media = await _progressMediaRepository.GetByIdAsync(id, ct);
            return media != null ? _mapper.Map<ProgressMediaDto>(media) : null;
        }

        public async Task<ProgressMediaListDto> GetByProjectIdAsync(Guid projectId, Guid? taskId, DateTime? fromDate, DateTime? toDate, int page = 1, int pageSize = 20, CancellationToken ct = default)
        {
            var items = await _progressMediaRepository.GetByProjectIdAsync(projectId, taskId, fromDate, toDate, page, pageSize, ct);
            var totalCount = await _progressMediaRepository.GetCountByProjectIdAsync(projectId, taskId, fromDate, toDate, ct);

            return new ProgressMediaListDto
            {
                Items = _mapper.Map<List<ProgressMediaDto>>(items),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<ProgressMediaDto> UploadAsync(Guid projectId, IFormFile file, string caption, Guid? taskId, Guid? progressUpdateId, Guid userId, CancellationToken ct = default)
        {
            // Kiểm tra quyền
            if (!await IsUserAuthorizedForProjectAsync(projectId, userId, ct))
                throw new UnauthorizedAccessException("Bạn không có quyền upload ảnh cho project này");

            // Upload file
            var fileUrl = await _fileService.UploadFileAsync(file.OpenReadStream(), file.FileName, $"projects/{projectId}/progress");

            // Tạo ProgressMedia
            var progressMedia = new ProgressMedia
            {
                ProjectId = projectId,
                TaskId = taskId,
                ProgressUpdateId = progressUpdateId,
                Url = fileUrl,
                Caption = caption,
                FileName = file.FileName,
                FileSize = file.Length,
                ContentType = file.ContentType,
                CreatorId = userId
            };

            await _progressMediaRepository.AddAsync(progressMedia, ct);
            await _progressMediaRepository.SaveChangesAsync(ct);

            return _mapper.Map<ProgressMediaDto>(progressMedia);
        }

        public async Task<bool> DeleteAsync(Guid id, Guid userId, CancellationToken ct = default)
        {
            var media = await _progressMediaRepository.GetByIdAsync(id, ct);
            if (media == null) return false;

            // Kiểm tra quyền (chỉ người upload hoặc có quyền project)
            if (media.CreatorId != userId && !await IsUserAuthorizedForProjectAsync(media.ProjectId, userId, ct))
                return false;

            // Xóa file vật lý
            await _fileService.DeleteFileAsync(media.Url);

            // Xóa record
            await _progressMediaRepository.DeleteAsync(media, ct);
            await _progressMediaRepository.SaveChangesAsync(ct);

            return true;
        }

        public async Task<bool> IsUserAuthorizedForProjectAsync(Guid projectId, Guid userId, CancellationToken ct = default)
        {
            var project = await _projectRepository.GetByIdAsync(projectId, ct);
            if (project == null) return false;

            // Kiểm tra Homeowner
            if (project.HomeownerId == userId) return true;

            // Kiểm tra Contractor (nếu có ContractorId)
            if (project.ContractorId == userId) return true;

            // Kiểm tra Supervisor (nếu có SupervisorId)
            if (project.SupervisorId == userId) return true;

            // Kiểm tra trong ProjectParticipants
            var isParticipant = project.Participants.Any(p => p.UserId == userId && p.Status == ParticipantStatus.Active);
            return isParticipant;
        }
    }
}
