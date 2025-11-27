// OCSP.Infrastructure/Data/Configurations/RegistrationRequestConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OCSP.Domain.Entities;

namespace OCSP.Infrastructure.Data.Configurations
{
    public class RegistrationRequestConfiguration : IEntityTypeConfiguration<RegistrationRequest>
    {
        public void Configure(EntityTypeBuilder<RegistrationRequest> b)
        {
            b.ToTable("RegistrationRequests");
            b.HasKey(x => x.Id);

            // Relations
            b.HasOne(x => x.ReviewedByUser)
             .WithMany()
             .HasForeignKey(x => x.ReviewedByUserId)
             .OnDelete(DeleteBehavior.SetNull);

            b.HasOne(x => x.CreatedUser)
             .WithMany()
             .HasForeignKey(x => x.CreatedUserId)
             .OnDelete(DeleteBehavior.SetNull);

            // Properties
            b.Property(x => x.Username)
             .IsRequired()
             .HasMaxLength(50);

            b.Property(x => x.Email)
             .IsRequired()
             .HasMaxLength(255);

            b.Property(x => x.Phone)
             .IsRequired()
             .HasMaxLength(20);

            b.Property(x => x.RequestedRole)
             .IsRequired()
             .HasConversion<int>();

            b.Property(x => x.Status)
             .IsRequired()
             .HasConversion<int>();

            b.Property(x => x.RejectionReason)
             .HasMaxLength(1000);

            // Supervisor fields
            b.Property(x => x.Department)
             .HasMaxLength(200);

            b.Property(x => x.Position)
             .HasMaxLength(200);

            b.Property(x => x.District)
             .HasMaxLength(100);

            // Contractor fields
            b.Property(x => x.CompanyName)
             .HasMaxLength(200);

            b.Property(x => x.BusinessLicense)
             .HasMaxLength(50);

            b.Property(x => x.TaxCode)
             .HasMaxLength(50);

            b.Property(x => x.Description)
             .HasMaxLength(2000);

            b.Property(x => x.Website)
             .HasMaxLength(500);

            b.Property(x => x.Address)
             .HasMaxLength(500);

            b.Property(x => x.City)
             .HasMaxLength(100);

            b.Property(x => x.Province)
             .HasMaxLength(100);

            // Indexes
            b.HasIndex(x => x.Email);
            b.HasIndex(x => x.Status);
            b.HasIndex(x => x.RequestedRole);
            b.HasIndex(x => x.CreatedAt);
        }
    }
}


