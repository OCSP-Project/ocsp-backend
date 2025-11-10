using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OCSP.Domain.Entities;

namespace OCSP.Infrastructure.Data.Configurations
{
    public class Project3DModelConfiguration : IEntityTypeConfiguration<Project3DModel>
    {
        public void Configure(EntityTypeBuilder<Project3DModel> builder)
        {
            builder.ToTable("Project3DModels");

            builder.HasKey(m => m.Id);

            builder.Property(m => m.FileName)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(m => m.FileUrl)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(m => m.FileSizeMB)
                .HasPrecision(10, 2);

            builder.Property(m => m.TotalMeshes)
                .HasDefaultValue(0);

            builder.Property(m => m.AnalysisCompleted)
                .HasDefaultValue(false);

            builder.Property(m => m.AnalysisResultJson)
                .HasColumnType("jsonb");

            // Relationships
            builder.HasOne(m => m.Project)
                .WithMany()
                .HasForeignKey(m => m.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(m => m.BuildingElements)
                .WithOne(e => e.Model)
                .HasForeignKey(e => e.ModelId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(m => m.MeshGroups)
                .WithOne(g => g.Model)
                .HasForeignKey(g => g.ModelId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(m => m.ProjectId);
            builder.HasIndex(m => m.AnalysisCompleted);
            builder.HasIndex(m => m.AnalyzedAt);
        }
    }
}

