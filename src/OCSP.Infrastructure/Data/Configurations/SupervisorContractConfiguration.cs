// OCSP.Infrastructure/Data/Configurations/SupervisorContractConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OCSP.Domain.Entities;

namespace OCSP.Infrastructure.Data.Configurations
{
    public class SupervisorContractConfiguration : IEntityTypeConfiguration<SupervisorContract>
    {
        public void Configure(EntityTypeBuilder<SupervisorContract> b)
        {
            b.ToTable("SupervisorContracts");
            b.HasKey(x => x.Id);

            // ---------------- Relations ----------------

            // 1 Project - N SupervisorContracts
            b.HasOne(x => x.Project)
             .WithMany()
             .HasForeignKey(x => x.ProjectId)
             .OnDelete(DeleteBehavior.Restrict);

            // 1 Supervisor - N SupervisorContracts
            b.HasOne(x => x.Supervisor)
             .WithMany()
             .HasForeignKey(x => x.SupervisorId)
             .OnDelete(DeleteBehavior.Restrict);

            // ---------------- Scalars ----------------
            b.Property(x => x.MonthlyPrice).HasColumnType("numeric(18,2)");

            b.Property(x => x.Terms)
             .HasMaxLength(10000)
             .HasDefaultValue(string.Empty);

            b.Property(x => x.Status)
             .HasConversion<int>()
             .IsRequired();

            // ---------------- Signature & PDF Fields ----------------
            b.Property(x => x.HomeownerSignatureBase64)
             .HasColumnName("homeownersignaturebase64")
             .HasMaxLength(1000000);

            b.Property(x => x.SupervisorSignatureBase64)
             .HasColumnName("supervisorsignaturebase64")
             .HasMaxLength(1000000);

            b.Property(x => x.TemplatePdfUrl)
             .HasColumnName("templatepdfurl")
             .HasMaxLength(1000);

            b.Property(x => x.SignedPdfUrl)
             .HasColumnName("signedpdfurl")
             .HasMaxLength(1000);

            // ---------------- Indexes ----------------
            b.HasIndex(x => x.ProjectId);
            b.HasIndex(x => x.SupervisorId);
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.HomeownerUserId);
            b.HasIndex(x => x.SupervisorUserId);
        }
    }
}




