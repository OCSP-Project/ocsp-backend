// OCSP.Infrastructure/Data/Configurations/ContractConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OCSP.Domain.Entities;
using OCSP.Domain.Enums;

namespace OCSP.Infrastructure.Data.Configurations
{
    public class ContractConfiguration : IEntityTypeConfiguration<Contract>
    {
        public void Configure(EntityTypeBuilder<Contract> b)
        {
            b.ToTable("Contracts");
            b.HasKey(x => x.Id);

            // FKs
            b.HasOne(x => x.Project)
             .WithMany()                    // hoặc .WithMany(p => p.Contracts) nếu có navigation ở Project
             .HasForeignKey(x => x.ProjectId)
             .OnDelete(DeleteBehavior.Restrict);

            // scalar
            b.Property(x => x.TotalPrice).HasColumnType("numeric(18,2)");
            b.Property(x => x.DurationDays).IsRequired();

            b.Property(x => x.Terms)
             .HasColumnType("varchar(2000)")
             .HasDefaultValue(string.Empty);

            b.Property(x => x.Status)
             .HasConversion<int>()         // enum -> int
             .IsRequired();

            // Index gợi ý
            b.HasIndex(x => x.ProjectId);
            b.HasIndex(x => x.Status);

            // Timestamps (AuditableEntity đã tự set trong SaveChanges)
        }
    }
}
