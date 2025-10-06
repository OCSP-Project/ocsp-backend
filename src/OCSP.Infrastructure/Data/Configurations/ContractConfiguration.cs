// OCSP.Infrastructure/Data/Configurations/ContractConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OCSP.Domain.Entities;

namespace OCSP.Infrastructure.Data.Configurations
{
    public class ContractConfiguration : IEntityTypeConfiguration<Contract>
    {
        public void Configure(EntityTypeBuilder<Contract> b)
        {
            b.ToTable("Contracts");
            b.HasKey(x => x.Id);

            // ---------------- Relations ----------------

            // 1 Project - N Contracts  (Project đã có ICollection<Contract> Contracts)
            b.HasOne(x => x.Project)
             .WithMany(p => p.Contracts)
             .HasForeignKey(x => x.ProjectId)
             .OnDelete(DeleteBehavior.Restrict);

            // 1 Contract - N Items
            b.HasMany(x => x.Items)
             .WithOne(i => i.Contract)
             .HasForeignKey(i => i.ContractId)
             .OnDelete(DeleteBehavior.Cascade);

            // 1 Contract - N Milestones
            b.HasMany(x => x.Milestones)
             .WithOne(m => m.Contract)
             .HasForeignKey(m => m.ContractId)
             .OnDelete(DeleteBehavior.Cascade);

            // 1 Contract - 1 Escrow (optional)
            b.HasOne(x => x.Escrow)
             .WithOne(e => e.Contract)
             .HasForeignKey<EscrowAccount>(e => e.ContractId)
             .OnDelete(DeleteBehavior.Cascade);

            // ---------------- Scalars ----------------
            b.Property(x => x.TotalPrice).HasColumnType("numeric(18,2)");
            b.Property(x => x.DurationDays).IsRequired();

            b.Property(x => x.Terms)
             .HasMaxLength(2000)        // để provider tự chọn kiểu (Postgres sẽ ra varchar(2000))
             .HasDefaultValue(string.Empty);

            b.Property(x => x.Status)
             .HasConversion<int>()      // enum -> int
             .IsRequired();

            // ---------------- Indexes ----------------
            b.HasIndex(x => x.ProjectId);
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.HomeownerUserId);
            b.HasIndex(x => x.ContractorUserId);
        }
    }
}
