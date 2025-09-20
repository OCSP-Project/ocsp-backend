using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OCSP.Application.DTOs.ProjectDailyResource;
using OCSP.Application.Services.Interfaces;
using OCSP.Domain.Entities;
using OCSP.Domain.Enums;
using OCSP.Infrastructure.Data;
using OCSP.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OCSP.Application.Services
{
    public class ProjectDailyResourceService : IProjectDailyResourceService
    {
        private readonly IProjectDailyResourceRepository _repository;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public ProjectDailyResourceService(
            IProjectDailyResourceRepository repository,
            ApplicationDbContext context,
            IMapper mapper)
        {
            _repository = repository;
            _context = context;
            _mapper = mapper;
        }

        public async Task<ProjectDailyResourceDto> CreateDailyResourceAsync(CreateProjectDailyResourceDto dto, Guid userId, CancellationToken cancellationToken = default)
        {
            // Kiểm tra quyền truy cập project
            if (!await CanUserAccessProjectAsync(dto.ProjectId, userId, cancellationToken))
            {
                throw new UnauthorizedAccessException("Bạn không có quyền truy cập project này");
            }

            // Kiểm tra xem đã có record cho ngày này chưa - TẠM THỜI COMMENT
            // var existingResource = await _repository.GetByProjectAndDateAsync(dto.ProjectId, dto.ResourceDate, cancellationToken);
            // if (existingResource != null)
            // {
            //     throw new InvalidOperationException($"Đã có báo cáo tài nguyên cho ngày {dto.ResourceDate:dd/MM/yyyy}");
            // }

            var entity = _mapper.Map<ProjectDailyResource>(dto);
            entity.CreatedBy = userId.ToString();
            entity.UpdatedBy = userId.ToString();

            var createdEntity = await _repository.CreateAsync(entity, cancellationToken);
            return _mapper.Map<ProjectDailyResourceDto>(createdEntity);
        }

        public async Task<ProjectDailyResourceDto> UpdateDailyResourceAsync(Guid dailyResourceId, UpdateProjectDailyResourceDto dto, Guid userId, CancellationToken cancellationToken = default)
        {
            // Kiểm tra quyền sửa
            if (!await CanUserModifyDailyResourceAsync(dailyResourceId, userId, cancellationToken))
            {
                throw new UnauthorizedAccessException("Bạn không có quyền sửa báo cáo tài nguyên này");
            }

            var entity = await _repository.GetByIdAsync(dailyResourceId, cancellationToken);
            if (entity == null)
            {
                throw new KeyNotFoundException("Không tìm thấy báo cáo tài nguyên");
            }

            _mapper.Map(dto, entity);
            entity.UpdatedBy = userId.ToString();

            var updatedEntity = await _repository.UpdateAsync(entity, cancellationToken);
            return _mapper.Map<ProjectDailyResourceDto>(updatedEntity);
        }

        public async Task<bool> DeleteDailyResourceAsync(Guid dailyResourceId, Guid userId, CancellationToken cancellationToken = default)
        {
            // Kiểm tra quyền xóa
            if (!await CanUserModifyDailyResourceAsync(dailyResourceId, userId, cancellationToken))
            {
                throw new UnauthorizedAccessException("Bạn không có quyền xóa báo cáo tài nguyên này");
            }

            return await _repository.DeleteAsync(dailyResourceId, cancellationToken);
        }

        public async Task<ProjectDailyResourceDto?> GetDailyResourceByIdAsync(Guid dailyResourceId, CancellationToken cancellationToken = default)
        {
            var entity = await _repository.GetByIdAsync(dailyResourceId, cancellationToken);
            if (entity == null) return null;

            // Load Project information separately to avoid SQL conflicts
            var project = await _context.Projects.FindAsync(entity.ProjectId);
            entity.Project = project;

            return _mapper.Map<ProjectDailyResourceDto>(entity);
        }

        public async Task<ProjectDailyResourceDto?> GetDailyResourceByProjectAndDateAsync(Guid projectId, DateTime resourceDate, CancellationToken cancellationToken = default)
        {
            var entity = await _repository.GetByProjectAndDateAsync(projectId, resourceDate, cancellationToken);
            if (entity == null) return null;

            // Load Project information separately to avoid SQL conflicts
            var project = await _context.Projects.FindAsync(entity.ProjectId);
            entity.Project = project;

            return _mapper.Map<ProjectDailyResourceDto>(entity);
        }

        public async Task<List<ProjectDailyResourceListDto>> GetDailyResourcesByProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
        {
            var entities = await _repository.GetByProjectIdAsync(projectId, cancellationToken);
            return entities.Select(e => new ProjectDailyResourceListDto
            {
                Id = e.Id,
                ResourceDate = e.ResourceDate,
                EquipmentCount = GetEquipmentCount(e),
                TotalCementConsumed = e.CementConsumed,
                TotalSandConsumed = e.SandConsumed,
                TotalAggregateConsumed = e.AggregateConsumed,
                TotalCementRemaining = e.CementRemaining,
                TotalSandRemaining = e.SandRemaining,
                TotalAggregateRemaining = e.AggregateRemaining,
                Notes = e.Notes,
                CreatedAt = e.CreatedAt,
                CreatedBy = e.CreatedBy
            }).ToList();
        }

        public async Task<List<ProjectDailyResourceListDto>> GetDailyResourcesByProjectAndDateRangeAsync(Guid projectId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            var entities = await _repository.GetByProjectIdAndDateRangeAsync(projectId, startDate, endDate, cancellationToken);
            return entities.Select(e => new ProjectDailyResourceListDto
            {
                Id = e.Id,
                ResourceDate = e.ResourceDate,
                EquipmentCount = GetEquipmentCount(e),
                TotalCementConsumed = e.CementConsumed,
                TotalSandConsumed = e.SandConsumed,
                TotalAggregateConsumed = e.AggregateConsumed,
                TotalCementRemaining = e.CementRemaining,
                TotalSandRemaining = e.SandRemaining,
                TotalAggregateRemaining = e.AggregateRemaining,
                Notes = e.Notes,
                CreatedAt = e.CreatedAt,
                CreatedBy = e.CreatedBy
            }).ToList();
        }

        public async Task<bool> CanUserAccessProjectAsync(Guid projectId, Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            // Admin có thể truy cập tất cả
            if (user.Role == UserRole.Admin) return true;

            // Homeowner có thể truy cập project của mình
            if (user.Role == UserRole.Homeowner)
            {
                var project = await _context.Projects.FindAsync(projectId);
                return project?.HomeownerId == userId;
            }

            // Supervisor và Contractor có thể truy cập project mà họ tham gia
            if (user.Role == UserRole.Supervisor || user.Role == UserRole.Contractor)
            {
                var isParticipant = await _context.ProjectParticipants
                    .AnyAsync(pp => pp.ProjectId == projectId && pp.UserId == userId, cancellationToken);
                
                if (isParticipant) return true;

                // Kiểm tra xem có phải supervisor/contractor chính của project không
                var project = await _context.Projects.FindAsync(projectId);
                if (project == null) return false;

                return (user.Role == UserRole.Supervisor && project.SupervisorId == userId) ||
                       (user.Role == UserRole.Contractor && project.ContractorId == userId);
            }

            return false;
        }

        public async Task<bool> CanUserModifyDailyResourceAsync(Guid dailyResourceId, Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return false;

            // Admin có thể sửa tất cả
            if (user.Role == UserRole.Admin) return true;

            // Homeowner không được sửa (chỉ xem)
            if (user.Role == UserRole.Homeowner) return false;

            // Supervisor và Contractor có thể sửa
            if (user.Role == UserRole.Supervisor || user.Role == UserRole.Contractor)
            {
                var dailyResource = await _repository.GetByIdAsync(dailyResourceId, cancellationToken);
                if (dailyResource == null) return false;

                return await CanUserAccessProjectAsync(dailyResource.ProjectId, userId, cancellationToken);
            }

            return false;
        }

        private static int GetEquipmentCount(ProjectDailyResource resource)
        {
            int count = 0;
            if (resource.TowerCrane) count++;
            if (resource.ConcreteMixer) count++;
            if (resource.MaterialHoist) count++;
            if (resource.PassengerHoist) count++;
            if (resource.Vibrator) count++;
            return count;
        }
    }
}
