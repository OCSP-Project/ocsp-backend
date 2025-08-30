using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OCSP.Domain.Entities;

namespace OCSP.Infrastructure.Data.Configurations
{
    public class ContractorConfiguration : IEntityTypeConfiguration<Contractor>
    {
        public void Configure(EntityTypeBuilder<Contractor> builder)
        {
            builder.ToTable("Contractors");
            
            builder.HasKey(c => c.Id);
            
            builder.Property(c => c.CompanyName)
                .IsRequired()
                .HasMaxLength(200);
            
            builder.Property(c => c.BusinessLicense)
                .IsRequired()
                .HasMaxLength(50);
            
            builder.Property(c => c.Description)
                .HasMaxLength(2000);
            
            builder.Property(c => c.Website)
                .HasMaxLength(500);
            
            builder.Property(c => c.ContactPhone)
                .HasMaxLength(20);
            
            builder.Property(c => c.ContactEmail)
                .HasMaxLength(200);
            
            builder.Property(c => c.Address)
                .HasMaxLength(500);
            
            builder.Property(c => c.City)
                .HasMaxLength(100);
            
            builder.Property(c => c.Province)
                .HasMaxLength(100);
            
            builder.Property(c => c.AverageRating)
                .HasPrecision(3, 2);
            
            builder.Property(c => c.MinProjectBudget)
                .HasPrecision(18, 2);
            
            builder.Property(c => c.MaxProjectBudget)
                .HasPrecision(18, 2);

            // Indexes for search optimization
            builder.HasIndex(c => c.CompanyName);
            builder.HasIndex(c => c.City);
            builder.HasIndex(c => c.AverageRating);
            builder.HasIndex(c => c.IsVerified);
            builder.HasIndex(c => c.IsPremium);
            builder.HasIndex(c => c.IsActive);
            builder.HasIndex(c => c.IsRestricted);
            
            // Composite indexes for common search patterns
            builder.HasIndex(c => new { c.IsActive, c.IsRestricted, c.IsVerified });
            builder.HasIndex(c => new { c.City, c.IsActive, c.AverageRating });
            
            // Relationships
            builder.HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasMany(c => c.Specialties)
                .WithOne(s => s.Contractor)
                .HasForeignKey(s => s.ContractorId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.HasMany(c => c.Documents)
                .WithOne(d => d.Contractor)
                .HasForeignKey(d => d.ContractorId)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.HasMany(c => c.Portfolios)
                .WithOne(p => p.Contractor)
                .HasForeignKey(p => p.ContractorId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class ContractorSpecialtyConfiguration : IEntityTypeConfiguration<ContractorSpecialty>
    {
        public void Configure(EntityTypeBuilder<ContractorSpecialty> builder)
        {
            builder.ToTable("ContractorSpecialties");
            
            builder.HasKey(s => s.Id);
            
            builder.Property(s => s.SpecialtyName)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.Property(s => s.Description)
                .HasMaxLength(500);
            
            builder.HasIndex(s => s.SpecialtyName);
            builder.HasIndex(s => s.ContractorId);
        }
    }

    public class ContractorDocumentConfiguration : IEntityTypeConfiguration<ContractorDocument>
    {
        public void Configure(EntityTypeBuilder<ContractorDocument> builder)
        {
            builder.ToTable("ContractorDocuments");
            
            builder.HasKey(d => d.Id);
            
            builder.Property(d => d.DocumentType)
                .IsRequired()
                .HasMaxLength(50);
            
            builder.Property(d => d.DocumentName)
                .IsRequired()
                .HasMaxLength(200);
            
            builder.Property(d => d.DocumentUrl)
                .IsRequired()
                .HasMaxLength(1000);
            
            builder.HasIndex(d => d.ContractorId);
            builder.HasIndex(d => d.DocumentType);
        }
    }

    public class ContractorPortfolioConfiguration : IEntityTypeConfiguration<ContractorPortfolio>
    {
        public void Configure(EntityTypeBuilder<ContractorPortfolio> builder)
        {
            builder.ToTable("ContractorPortfolios");
            
            builder.HasKey(p => p.Id);
            
            builder.Property(p => p.ProjectName)
                .IsRequired()
                .HasMaxLength(200);
            
            builder.Property(p => p.ProjectDescription)
                .HasMaxLength(1000);
            
            builder.Property(p => p.ImageUrl)
                .HasMaxLength(1000);
            
            builder.Property(p => p.ProjectValue)
                .HasPrecision(18, 2);
            
            builder.Property(p => p.ClientTestimonial)
                .HasMaxLength(1000);
            
            builder.HasIndex(p => p.ContractorId);
            builder.HasIndex(p => new { p.ContractorId, p.DisplayOrder });
        }
    }

    public class CommunicationConfiguration : IEntityTypeConfiguration<Communication>
    {
        public void Configure(EntityTypeBuilder<Communication> builder)
        {
            builder.ToTable("Communications");
            
            builder.HasKey(c => c.Id);
            
            builder.Property(c => c.Content)
                .IsRequired()
                .HasMaxLength(4000);
            
            builder.Property(c => c.Type)
                .HasConversion<int>();
            
            builder.Property(c => c.FlagReason)
                .HasMaxLength(500);
            
            builder.HasIndex(c => c.FromUserId);
            builder.HasIndex(c => c.ToUserId);
            builder.HasIndex(c => c.ProjectId);
            builder.HasIndex(c => c.IsFlagged);
            builder.HasIndex(c => c.CreatedAt);
            
            // Relationships
            builder.HasOne(c => c.FromUser)
                .WithMany()
                .HasForeignKey(c => c.FromUserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasOne(c => c.ToUser)
                .WithMany()
                .HasForeignKey(c => c.ToUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}