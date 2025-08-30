using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OCSP.Domain.Entities;

namespace OCSP.Infrastructure.Data.Configurations
{
    public class ContractConfiguration : IEntityTypeConfiguration<Contract>
    {
        public void Configure(EntityTypeBuilder<Contract> builder)
        {
            builder.ToTable("Contracts");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Title)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(c => c.Status)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(c => c.Value)
                   .HasPrecision(18, 2);

            // Contract → Project
            builder.HasOne(c => c.Project)
                   .WithMany(p => p.Contracts!) // nếu Project có ICollection<Contract> Contracts; nếu không có, thay .WithMany()
                   .HasForeignKey(c => c.ProjectId)
                   .OnDelete(DeleteBehavior.Restrict);

            // ✅ Contract → Contractor (FK = ContractorId)
            builder.HasOne(c => c.Contractor)
                   .WithMany(x => x.Contracts)
                   .HasForeignKey(c => c.ContractorId)
                   .OnDelete(DeleteBehavior.Restrict);

            // ✅ (Tuỳ chọn) Contract → User (Homeowner) (FK = HomeownerId)
            // Nếu bạn không dùng Homeowner, xoá 2 dòng property HomeownerId/Homeowner trong entity Contract,
            // và comment block dưới đây.
            builder.HasOne(c => c.Homeowner)
                   .WithMany() // hoặc .WithMany(u => u.ContractsAsHomeowner) nếu bạn có collection này ở User
                   .HasForeignKey(c => c.HomeownerId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Indexes hữu ích
            builder.HasIndex(c => c.ProjectId);
            builder.HasIndex(c => c.ContractorId);
            builder.HasIndex(c => c.Status);
            builder.HasIndex(c => c.StartDate);
            builder.HasIndex(c => c.EndDate);
        }
    }
}
