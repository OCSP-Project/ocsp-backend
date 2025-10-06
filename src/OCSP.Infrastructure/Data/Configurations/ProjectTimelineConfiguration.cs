using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OCSP.Domain.Entities;

namespace OCSP.Infrastructure.Data.Configurations
{
    public class ProjectTimelineConfiguration : IEntityTypeConfiguration<ProjectTimeline>
    {
        public void Configure(EntityTypeBuilder<ProjectTimeline> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Description).HasMaxLength(1000);
            builder.Property(x => x.Status).HasConversion<int>();

            builder.HasOne(x => x.Project)
                .WithMany()
                .HasForeignKey(x => x.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Contractor)
                .WithMany()
                .HasForeignKey(x => x.ContractorId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class MilestoneConfiguration : IEntityTypeConfiguration<Milestone>
    {
        public void Configure(EntityTypeBuilder<Milestone> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Description).HasMaxLength(1000);
            builder.Property(x => x.Status).HasConversion<int>();
            builder.Property(x => x.ProgressPercentage).HasColumnType("decimal(5,2)");

            builder.HasOne(x => x.ProjectTimeline)
                .WithMany(x => x.Milestones)
                .HasForeignKey(x => x.ProjectTimelineId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class DeliverableConfiguration : IEntityTypeConfiguration<Deliverable>
    {
        public void Configure(EntityTypeBuilder<Deliverable> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Description).HasMaxLength(1000);
            builder.Property(x => x.Status).HasConversion<int>();
            builder.Property(x => x.ProgressPercentage).HasColumnType("decimal(5,2)");

            builder.HasOne(x => x.Milestone)
                .WithMany(x => x.Deliverables)
                .HasForeignKey(x => x.MilestoneId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
