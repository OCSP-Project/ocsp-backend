using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OCSP.Domain.Entities;

namespace OCSP.Infrastructure.Data.Configurations
{
    public class ConstructionDiaryConfiguration : IEntityTypeConfiguration<ConstructionDiary>
    {
        public void Configure(EntityTypeBuilder<ConstructionDiary> b)
        {
            b.ToTable("ConstructionDiaries");
            b.HasKey(x => x.Id);

            // Relations
            b.HasOne(x => x.Project)
             .WithMany()
             .HasForeignKey(x => x.ProjectId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasMany(x => x.WorkItems)
             .WithOne(w => w.ConstructionDiary)
             .HasForeignKey(w => w.ConstructionDiaryId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasMany(x => x.WeatherPeriods)
             .WithOne(w => w.ConstructionDiary)
             .HasForeignKey(w => w.ConstructionDiaryId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasMany(x => x.Images)
             .WithOne(i => i.ConstructionDiary)
             .HasForeignKey(i => i.ConstructionDiaryId)
             .OnDelete(DeleteBehavior.Cascade);

            // Properties
            b.Property(x => x.DiaryDate)
             .IsRequired();

            b.Property(x => x.ConstructionTeam)
             .HasMaxLength(200);

            // Ratings as enum -> int
            b.Property(x => x.SafetyRating)
             .HasConversion<int>()
             .IsRequired();

            b.Property(x => x.QualityRating)
             .HasConversion<int>()
             .IsRequired();

            b.Property(x => x.ProgressRating)
             .HasConversion<int>()
             .IsRequired();

            b.Property(x => x.CleanlinessRating)
             .HasConversion<int>()
             .IsRequired();

            // Long text fields
            b.Property(x => x.IncidentReport)
             .HasColumnType("text");

            b.Property(x => x.Recommendations)
             .HasColumnType("text");

            b.Property(x => x.Notes)
             .HasColumnType("text");

            b.Property(x => x.SupervisorName)
             .HasMaxLength(200);

            b.Property(x => x.SupervisorPosition)
             .HasMaxLength(200);

            b.Property(x => x.ContractorName)
             .HasMaxLength(200);

            b.Property(x => x.SupervisorUnitName)
             .HasMaxLength(200);

            // Indexes
            b.HasIndex(x => new { x.ProjectId, x.DiaryDate }).IsUnique();
            b.HasIndex(x => x.DiaryDate);
        }
    }

    public class DiaryWorkItemConfiguration : IEntityTypeConfiguration<DiaryWorkItem>
    {
        public void Configure(EntityTypeBuilder<DiaryWorkItem> b)
        {
            b.ToTable("DiaryWorkItems");
            b.HasKey(x => x.Id);

            // Relations
            b.HasOne(x => x.ConstructionDiary)
             .WithMany(d => d.WorkItems)
             .HasForeignKey(x => x.ConstructionDiaryId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.WorkItem)
             .WithMany()
             .HasForeignKey(x => x.WorkItemId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasMany(x => x.LaborEntries)
             .WithOne(l => l.DiaryWorkItem)
             .HasForeignKey(l => l.DiaryWorkItemId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasMany(x => x.EquipmentEntries)
             .WithOne(e => e.DiaryWorkItem)
             .HasForeignKey(e => e.DiaryWorkItemId)
             .OnDelete(DeleteBehavior.Cascade);

            // Properties
            b.Property(x => x.WorkItemName)
             .IsRequired()
             .HasMaxLength(500);

            b.Property(x => x.ConstructionArea)
             .HasMaxLength(200);

            b.Property(x => x.PlannedQuantity)
             .HasColumnType("decimal(18,2)");

            b.Property(x => x.ConstructedQuantity)
             .HasColumnType("decimal(18,2)");

            b.Property(x => x.RemainingQuantity)
             .HasColumnType("decimal(18,2)");

            b.Property(x => x.Unit)
             .IsRequired()
             .HasMaxLength(50);

            // Indexes
            b.HasIndex(x => x.ConstructionDiaryId);
            b.HasIndex(x => x.WorkItemId);
        }
    }

    public class DiaryLaborConfiguration : IEntityTypeConfiguration<DiaryLabor>
    {
        public void Configure(EntityTypeBuilder<DiaryLabor> b)
        {
            b.ToTable("DiaryLabors");
            b.HasKey(x => x.Id);

            // Relations
            b.HasOne(x => x.DiaryWorkItem)
             .WithMany(w => w.LaborEntries)
             .HasForeignKey(x => x.DiaryWorkItemId)
             .OnDelete(DeleteBehavior.Cascade);

            // Properties
            b.Property(x => x.LaborName)
             .IsRequired()
             .HasMaxLength(200);

            b.Property(x => x.Position)
             .HasMaxLength(100);

            b.Property(x => x.WorkHours)
             .IsRequired()
             .HasMaxLength(50);

            b.Property(x => x.Team)
             .IsRequired()
             .HasMaxLength(100);

            b.Property(x => x.Shift)
             .IsRequired()
             .HasMaxLength(100);

            b.Property(x => x.Quantity)
             .HasColumnType("decimal(18,2)");

            b.Property(x => x.Unit)
             .IsRequired()
             .HasMaxLength(50);

            // Indexes
            b.HasIndex(x => x.DiaryWorkItemId);
        }
    }

    public class DiaryEquipmentConfiguration : IEntityTypeConfiguration<DiaryEquipment>
    {
        public void Configure(EntityTypeBuilder<DiaryEquipment> b)
        {
            b.ToTable("DiaryEquipments");
            b.HasKey(x => x.Id);

            // Relations
            b.HasOne(x => x.DiaryWorkItem)
             .WithMany(w => w.EquipmentEntries)
             .HasForeignKey(x => x.DiaryWorkItemId)
             .OnDelete(DeleteBehavior.Cascade);

            // Properties
            b.Property(x => x.EquipmentName)
             .IsRequired()
             .HasMaxLength(200);

            b.Property(x => x.Specifications)
             .IsRequired()
             .HasMaxLength(500);

            b.Property(x => x.HoursUsed)
             .HasColumnType("decimal(18,4)");

            b.Property(x => x.Quantity)
             .HasColumnType("decimal(18,2)");

            b.Property(x => x.Unit)
             .IsRequired()
             .HasMaxLength(50);

            // Indexes
            b.HasIndex(x => x.DiaryWorkItemId);
        }
    }

    public class DiaryWeatherPeriodConfiguration : IEntityTypeConfiguration<DiaryWeatherPeriod>
    {
        public void Configure(EntityTypeBuilder<DiaryWeatherPeriod> b)
        {
            b.ToTable("DiaryWeatherPeriods");
            b.HasKey(x => x.Id);

            // Relations
            b.HasOne(x => x.ConstructionDiary)
             .WithMany(d => d.WeatherPeriods)
             .HasForeignKey(x => x.ConstructionDiaryId)
             .OnDelete(DeleteBehavior.Cascade);

            // Properties
            b.Property(x => x.Period)
             .IsRequired()
             .HasMaxLength(50);

            b.Property(x => x.Condition)
             .IsRequired()
             .HasMaxLength(100);

            b.Property(x => x.Temperature)
             .HasMaxLength(50);

            // Indexes
            b.HasIndex(x => x.ConstructionDiaryId);
        }
    }

    public class DiaryImageConfiguration : IEntityTypeConfiguration<DiaryImage>
    {
        public void Configure(EntityTypeBuilder<DiaryImage> b)
        {
            b.ToTable("DiaryImages");
            b.HasKey(x => x.Id);

            // Relations
            b.HasOne(x => x.ConstructionDiary)
             .WithMany(d => d.Images)
             .HasForeignKey(x => x.ConstructionDiaryId)
             .OnDelete(DeleteBehavior.Cascade);

            // Properties
            b.Property(x => x.Url)
             .IsRequired()
             .HasMaxLength(2000); // For S3 URLs or base64

            b.Property(x => x.Category)
             .HasConversion<int>()
             .IsRequired();

            b.Property(x => x.Description)
             .HasMaxLength(500);

            b.Property(x => x.UploadedAt)
             .IsRequired();

            // Indexes
            b.HasIndex(x => x.ConstructionDiaryId);
            b.HasIndex(x => x.Category);
            b.HasIndex(x => x.UploadedAt);
        }
    }
}
