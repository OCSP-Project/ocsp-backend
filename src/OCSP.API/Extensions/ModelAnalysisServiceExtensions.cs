using Microsoft.Extensions.DependencyInjection;
using OCSP.Application.Services;
using OCSP.Application.Services.Interfaces;
using OCSP.Infrastructure.Repositories;
using OCSP.Infrastructure.Repositories.Interfaces;
using OCSP.Infrastructure.Services;

namespace OCSP.API.Extensions
{
    public static class ModelAnalysisServiceExtensions
    {
        /// <summary>
        /// Register Model Analysis services
        /// </summary>
        public static IServiceCollection AddModelAnalysisServices(this IServiceCollection services)
        {
            // ==================== REPOSITORIES ====================

            // Model
            services.AddScoped<IProject3DModelRepository, Project3DModelRepository>();
            // Building Elements + Tracking
            services.AddScoped<IBuildingElementRepository, BuildingElementRepository>();
            services.AddScoped<IElementTrackingHistoryRepository, ElementTrackingHistoryRepository>();
            services.AddScoped<ITrackingPhotoRepository, TrackingPhotoRepository>();

            // ==================== SERVICES ====================

            services.AddScoped<IModelAnalysisService, ModelAnalysisService>();
            services.AddScoped<IGLBValidatorService, GLBValidatorService>();
            services.AddScoped<IBuildingElementService, BuildingElementService>();

            // File Storage (local for demo, Azure Blob for production)
            services.AddScoped<IFileStorageService, LocalFileStorageService>();
            // For production: services.AddScoped<IFileStorageService, AzureBlobStorageService>();

            return services;
        }
    }
}
