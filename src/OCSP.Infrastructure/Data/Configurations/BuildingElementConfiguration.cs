using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OCSP.Domain.Entities;
using OCSP.Domain.Enums;

namespace OCSP.Infrastructure.Data.Configurations
{
    public class BuildingElementConfiguration : IEntityTypeConfiguration<BuildingElement>
    {
        public void Configure(EntityTypeBuilder<BuildingElement> builder)
        {
            builder.ToTable("BuildingElements");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(e => e.ElementType)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(e => e.TrackingStatus)
                .IsRequired()
                .HasConversion<int>();

            // Dimensions with precision
            builder.Property(e => e.Width).HasPrecision(10, 3);
            builder.Property(e => e.Length).HasPrecision(10, 3);
            builder.Property(e => e.Height).HasPrecision(10, 3);
            builder.Property(e => e.CenterX).HasPrecision(10, 3);
            builder.Property(e => e.CenterY).HasPrecision(10, 3);
            builder.Property(e => e.CenterZ).HasPrecision(10, 3);
            builder.Property(e => e.VolumeM3).HasPrecision(12, 3);
            builder.Property(e => e.CompletionPercentage).HasPrecision(5, 2);

            builder.Property(e => e.MeshIndices)
                .HasColumnType("jsonb"); // PostgreSQL

            builder.Property(e => e.Color)
                .HasMaxLength(7)
                .HasDefaultValue("#CCCCCC");

            builder.Property(e => e.TrackingStatus).HasDefaultValue(TrackingStatus.NotStarted);
            builder.Property(e => e.CompletionPercentage).HasDefaultValue(0);
            builder.Property(e => e.CanTrack).HasDefaultValue(true);

            // Relationships
            builder.HasOne(e => e.Model)
                .WithMany(m => m.BuildingElements)
                .HasForeignKey(e => e.ModelId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(e => e.TrackingHistory)
                .WithOne(h => h.BuildingElement)
                .HasForeignKey(h => h.BuildingElementId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.HasIndex(e => e.ModelId);
            builder.HasIndex(e => e.ElementType);
            builder.HasIndex(e => e.TrackingStatus);
            builder.HasIndex(e => e.FloorLevel);
            builder.HasIndex(e => new { e.ModelId, e.CompletionPercentage });
            builder.HasIndex(e => new { e.ModelId, e.TrackingStatus });
        }
    }

    public class MeshGroupConfiguration : IEntityTypeConfiguration<MeshGroup>
    {
        public void Configure(EntityTypeBuilder<MeshGroup> builder)
        {
            builder.ToTable("MeshGroups");
            builder.HasKey(g => g.Id);

            builder.Property(g => g.ComponentType).HasConversion<int>();
            builder.Property(g => g.MeshIndicesJson).HasColumnType("jsonb");
            builder.Property(g => g.Color).HasMaxLength(7); // #RRGGBB
            builder.Property(g => g.VolumeM3).HasPrecision(12, 3);
            builder.Property(g => g.Unit).HasMaxLength(10);

            builder.Property(g => g.MeshIndicesJson).HasDefaultValue("[]");
            builder.Property(g => g.Color).HasDefaultValue("#CCCCCC");
            builder.Property(g => g.Unit).HasDefaultValue("m3");
            builder.Property(g => g.IsAutoDetected).HasDefaultValue(true);

            builder.Property(g => g.DetectionAlgorithm).HasMaxLength(100);

            builder.HasOne(g => g.Model)
                .WithMany(m => m.MeshGroups)
                .HasForeignKey(g => g.ModelId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(g => g.ModelId);
            builder.HasIndex(g => g.ComponentType);
        }
    }

    public class ElementTrackingHistoryConfiguration : IEntityTypeConfiguration<ElementTrackingHistory>
    {
        public void Configure(EntityTypeBuilder<ElementTrackingHistory> builder)
        {
            builder.ToTable("ElementTrackingHistory");
            builder.HasKey(h => h.Id);

            builder.Property(h => h.TrackingDate).IsRequired();
            builder.Property(h => h.NewStatus).HasConversion<int>();
            builder.Property(h => h.PreviousStatus).HasConversion<int>();
            builder.Property(h => h.PreviousPercentage).HasPrecision(5, 2);
            builder.Property(h => h.NewPercentage).HasPrecision(5, 2);
            builder.Property(h => h.PlannedQuantity).HasPrecision(10, 2);
            builder.Property(h => h.ActualQuantity).HasPrecision(10, 2);
            builder.Property(h => h.CementUsed).HasPrecision(10, 2);
            builder.Property(h => h.SandUsed).HasPrecision(10, 2);
            builder.Property(h => h.AggregateUsed).HasPrecision(10, 2);
            builder.Property(h => h.Notes).HasMaxLength(2000);

            builder.HasOne(h => h.BuildingElement)
                .WithMany(e => e.TrackingHistory)
                .HasForeignKey(h => h.BuildingElementId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(h => h.RecordedBy)
                .WithMany()
                .HasForeignKey(h => h.RecordedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(h => h.Photos)
                .WithOne(p => p.TrackingHistory)
                .HasForeignKey(p => p.TrackingHistoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(h => h.BuildingElementId);
            builder.HasIndex(h => h.TrackingDate);
            builder.HasIndex(h => h.RecordedById);
            builder.HasIndex(h => new { h.BuildingElementId, h.TrackingDate });
            builder.HasIndex(h => new { h.BuildingElementId, h.NewStatus });
        }
    }

    public class TrackingPhotoConfiguration : IEntityTypeConfiguration<TrackingPhoto>
    {
        public void Configure(EntityTypeBuilder<TrackingPhoto> builder)
        {
            builder.ToTable("TrackingPhotos");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.PhotoUrl).IsRequired().HasMaxLength(2000);
            builder.Property(p => p.Caption).HasMaxLength(500);
            builder.Property(p => p.FileSizeMB).HasPrecision(10, 2);
            builder.Property(p => p.FileType).HasMaxLength(50);

            builder.HasOne(p => p.TrackingHistory)
                .WithMany(h => h.Photos)
                .HasForeignKey(p => p.TrackingHistoryId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(p => p.TrackingHistoryId);
            builder.HasIndex(p => p.UploadedAt);
        }
    }
}

