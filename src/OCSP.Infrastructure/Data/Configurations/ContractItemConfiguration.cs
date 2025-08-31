// OCSP.Infrastructure/Data/Configurations/ContractItemConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OCSP.Domain.Entities;

namespace OCSP.Infrastructure.Data.Configurations
{
    public class ContractItemConfiguration : IEntityTypeConfiguration<ContractItem>
    {
        public void Configure(EntityTypeBuilder<ContractItem> b)
        {
            b.ToTable("ContractItems");
            b.HasKey(x => x.Id);

            b.HasOne(x => x.Contract)
             .WithMany(c => c.Items)
             .HasForeignKey(x => x.ContractId)
             .OnDelete(DeleteBehavior.Cascade);

            b.Property(x => x.Name)
             .HasMaxLength(300)
             .IsRequired();

            b.Property(x => x.Unit)
             .HasMaxLength(50)
             .IsRequired();

            b.Property(x => x.Qty)
             .HasColumnType("numeric(18,2)")
             .IsRequired();

            b.Property(x => x.UnitPrice)
             .HasColumnType("numeric(18,2)")
             .IsRequired();

            // Index gợi ý
            b.HasIndex(x => x.ContractId);
        }
    }
}
