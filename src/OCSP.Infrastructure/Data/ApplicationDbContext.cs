//════════════════════════════════════════════════════════
// OCSP.Infrastructure/Data/ApplicationDbContext.cs
//════════════════════════════════════════════════════════

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OCSP.Domain.Entities;
using OCSP.Infrastructure.Data.Configurations;

namespace OCSP.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Existing entities
        public DbSet<User> Users { get; set; }
        public DbSet<Supervisor> Supervisors { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<ConversationParticipant> ConversationParticipants { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<ProfileDocument> ProfileDocuments { get; set; }

        // NEW: Contract
        public DbSet<Contract> Contracts { get; set; }

        // Contractor-related entities
        public DbSet<Contractor> Contractors { get; set; }
        public DbSet<ContractorSpecialty> ContractorSpecialties { get; set; }
        public DbSet<ContractorDocument> ContractorDocuments { get; set; }
        public DbSet<ContractorPortfolio> ContractorPortfolios { get; set; }
        public DbSet<Communication> Communications { get; set; }
        public DbSet<Review> Reviews { get; set; } // Add if not exists

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apply contractor configurations
            modelBuilder.ApplyConfiguration(new ContractorConfiguration());
            modelBuilder.ApplyConfiguration(new ContractorSpecialtyConfiguration());
            modelBuilder.ApplyConfiguration(new ContractorDocumentConfiguration());
            modelBuilder.ApplyConfiguration(new ContractorPortfolioConfiguration());
            modelBuilder.ApplyConfiguration(new CommunicationConfiguration());

            // ✅ Apply contract configuration (quan trọng để dẹp lỗi mơ hồ FK)
            modelBuilder.ApplyConfiguration(new ContractConfiguration());

            // Existing User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Username)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.Email)
                      .IsRequired()
                      .HasMaxLength(255);

                entity.Property(e => e.PasswordHash)
                      .IsRequired();

                entity.Property(e => e.Role)
                      .IsRequired()
                      .HasConversion<int>();

                entity.Property(e => e.IsEmailVerified)
                      .HasDefaultValue(false);

                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.Username).IsUnique();
            });

            // Existing Supervisor configuration
            modelBuilder.Entity<Supervisor>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Department).HasMaxLength(100);
                entity.Property(e => e.Position).HasMaxLength(100);
                entity.Property(e => e.Phone).HasMaxLength(30);

                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .IsRequired();
            });

            // Existing Project configuration
            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Status).HasMaxLength(50);

                entity.HasOne(e => e.Supervisor)
                      .WithMany()
                      .HasForeignKey(e => e.SupervisorId)
                      .IsRequired();

                // Nếu Project có ICollection<Contract> Contracts, bạn có thể thêm:
                // entity.HasMany(p => p.Contracts)
                //       .WithOne(c => c.Project)
                //       .HasForeignKey(c => c.ProjectId)
                //       .OnDelete(DeleteBehavior.Restrict);
            });

            // Existing Conversation configuration
            modelBuilder.Entity<Conversation>(e =>
            {
                e.HasKey(x => x.Id);
                e.HasOne(x => x.Project)
                 .WithMany(p => p.Conversations!)
                 .HasForeignKey(x => x.ProjectId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // Existing ConversationParticipant configuration
            modelBuilder.Entity<ConversationParticipant>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(cp => cp.User)
                      .WithMany(u => u.Conversations)
                      .HasForeignKey(cp => cp.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(cp => cp.Conversation)
                      .WithMany(c => c.Participants)
                      .HasForeignKey(cp => cp.ConversationId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Existing ChatMessage configuration
            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Content)
                      .IsRequired()
                      .HasMaxLength(1000);

                entity.HasOne(m => m.Conversation)
                      .WithMany(c => c.Messages)
                      .HasForeignKey(m => m.ConversationId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(m => m.Sender)
                      .WithMany(u => u.Messages)
                      .HasForeignKey(m => m.SenderId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Existing Profile configuration
            modelBuilder.Entity<Profile>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.FirstName).HasMaxLength(100);
                entity.Property(e => e.LastName).HasMaxLength(100);
                entity.Property(e => e.PhoneNumber).HasMaxLength(20);
                entity.Property(e => e.Address).HasMaxLength(500);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.State).HasMaxLength(100);
                entity.Property(e => e.Country).HasMaxLength(100);
                entity.Property(e => e.Bio).HasMaxLength(1000);
                entity.Property(e => e.AvatarUrl).HasMaxLength(500);

                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Existing ProfileDocument configuration
            modelBuilder.Entity<ProfileDocument>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.FileName)
                      .IsRequired()
                      .HasMaxLength(255);

                entity.Property(e => e.FileUrl)
                      .IsRequired()
                      .HasMaxLength(500);

                entity.Property(e => e.FileType)
                      .IsRequired()
                      .HasMaxLength(20);

                entity.Property(e => e.DocumentType)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(e => e.Description)
                      .HasMaxLength(500);

                entity.HasOne(e => e.Profile)
                      .WithMany(p => p.ProfileDocuments)
                      .HasForeignKey(e => e.ProfileId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Review entity configuration
            modelBuilder.Entity<Review>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Rating)
                      .IsRequired()
                      .HasColumnType("int");

                entity.Property(e => e.Comment)
                      .HasMaxLength(1000);

                entity.HasOne(e => e.Reviewer)
                      .WithMany()
                      .HasForeignKey(e => e.ReviewerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Contractor)
                      .WithMany(c => c.ReceivedReviews)
                      .HasForeignKey(e => e.ContractorId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Project)
                      .WithMany()
                      .HasForeignKey(e => e.ProjectId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // Handle User entities
            var userEntries = ChangeTracker.Entries<User>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in userEntries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                }
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }

            // Handle AuditableEntity-like (CreatedAt/UpdatedAt) — reflection cách nhanh gọn
            var auditableEntries = ChangeTracker.Entries()
                .Where(e => e.Entity.GetType().GetProperty("CreatedAt") != null &&
                           (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in auditableEntries)
            {
                var entity = entry.Entity;
                var createdAtProperty = entity.GetType().GetProperty("CreatedAt");
                var updatedAtProperty = entity.GetType().GetProperty("UpdatedAt");

                if (entry.State == EntityState.Added && createdAtProperty != null)
                {
                    createdAtProperty.SetValue(entity, DateTime.UtcNow);
                }

                if (updatedAtProperty != null)
                {
                    updatedAtProperty.SetValue(entity, DateTime.UtcNow);
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
