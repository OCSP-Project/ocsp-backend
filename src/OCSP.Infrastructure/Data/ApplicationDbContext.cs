//════════════════════════════════════════════════════════
// OCSP.Infrastructure/Data/ApplicationDbContext.cs
//════════════════════════════════════════════════════════

using Microsoft.EntityFrameworkCore;
using OCSP.Domain.Entities;

namespace OCSP.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Supervisor> Supervisors { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
    public DbSet<ConversationParticipant> ConversationParticipants { get; set; }
    public DbSet<ChatMessage> ChatMessages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User
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

            // Supervisor
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

            // Project
            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Status).HasMaxLength(50);

                entity.HasOne(e => e.Supervisor)
                      .WithMany(s => s.Projects!)
                      .HasForeignKey(e => e.SupervisorId)
                      .IsRequired();
            });
            // Conversation
modelBuilder.Entity<Conversation>(e =>
{
    e.HasKey(x => x.Id);
    e.HasOne(x => x.Project)
     .WithMany(p => p.Conversations!)
     .HasForeignKey(x => x.ProjectId)
     .OnDelete(DeleteBehavior.Cascade); 
});

// ConversationParticipant
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

// ChatMessage
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
          .OnDelete(DeleteBehavior.Restrict); // tránh xóa user làm mất hết message
});

        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<User>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                }
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}