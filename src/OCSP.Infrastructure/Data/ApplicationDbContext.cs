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
            // NEW
            public DbSet<ProjectParticipant> ProjectParticipants { get; set; }
            public DbSet<QuoteRequest> QuoteRequests { get; set; }
            public DbSet<QuoteInvite> QuoteInvites { get; set; }
            public DbSet<Proposal> Proposals { get; set; }
            public DbSet<ProposalItem> ProposalItems { get; set; }


            // NEW: Project Documents
            public DbSet<ProjectDocument> ProjectDocuments { get; set; }
            public DbSet<PermitMetadata> PermitMetadata { get; set; }
            // NEW: Contract
            public DbSet<Contract> Contracts { get; set; }
            public DbSet<ContractItem> ContractItems { get; set; }
            // NEW: Milestone & Escrow
            public DbSet<ContractMilestone> ContractMilestones { get; set; }
            public DbSet<EscrowAccount> EscrowAccounts { get; set; }
            public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
            public DbSet<Wallet> Wallets { get; set; }
            public DbSet<WalletTransaction> WalletTransactions { get; set; }
            public DbSet<LedgerEntry> LedgerEntries { get; set; }


            // Contractor-related entities
            public DbSet<Contractor> Contractors { get; set; }
            public DbSet<ContractorSpecialty> ContractorSpecialties { get; set; }
            public DbSet<ContractorDocument> ContractorDocuments { get; set; }
            public DbSet<ContractorPortfolio> ContractorPortfolios { get; set; }
            public DbSet<Communication> Communications { get; set; }
            public DbSet<Review> Reviews { get; set; } // Add if not exists
            public DbSet<ProjectTimeline> ProjectTimelines { get; set; }
            public DbSet<Milestone> Milestones { get; set; }
            public DbSet<Deliverable> Deliverables { get; set; }

            public DbSet<ProgressMedia> ProgressMedias { get; set; }

            // NEW: Project Daily Resources
            public DbSet<ProjectDailyResource> ProjectDailyResources { get; set; }
            public DbSet<ContractorPost> ContractorPosts { get; set; }
            public DbSet<ContractorPostImage> ContractorPostImages { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                  base.OnModelCreating(modelBuilder);

                  // Apply contractor configurations
                  modelBuilder.ApplyConfiguration(new ContractorConfiguration());
                  modelBuilder.ApplyConfiguration(new ContractorSpecialtyConfiguration());
                  modelBuilder.ApplyConfiguration(new ContractorDocumentConfiguration());
                  modelBuilder.ApplyConfiguration(new ContractorPortfolioConfiguration());
                  modelBuilder.ApplyConfiguration(new CommunicationConfiguration());

                  // Apply project timeline configurations
                  modelBuilder.ApplyConfiguration(new ProjectTimelineConfiguration());
                  modelBuilder.ApplyConfiguration(new MilestoneConfiguration());
                  modelBuilder.ApplyConfiguration(new DeliverableConfiguration());

                  // ✅ Apply contract configuration (quan trọng để dẹp lỗi mơ hồ FK)
                  modelBuilder.ApplyConfiguration(new ContractConfiguration());
                  modelBuilder.ApplyConfiguration(new ContractItemConfiguration());
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
                  // NEW: Project Documents
                  modelBuilder.Entity<ProjectDocument>(entity =>
              {
                    entity.HasKey(e => e.Id);

                    entity.Property(e => e.FileName)
                    .IsRequired()
                    .HasMaxLength(255);

                    entity.Property(e => e.FileUrl)
                    .IsRequired()
                    .HasMaxLength(500);

                    entity.Property(e => e.FileHash)
                    .IsRequired()
                    .HasMaxLength(64); // SHA256

                    entity.Property(e => e.DocumentType)
                    .HasConversion<int>();

                    // FK to Project
                    entity.HasOne(e => e.Project)
                    .WithMany(p => p.Documents) // Thêm navigation property vào Project
                    .HasForeignKey(e => e.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);

                    // FK to User (uploader)
                    entity.HasOne(e => e.UploadedBy)
                    .WithMany()
                    .HasForeignKey(e => e.UploadedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);

                    // Indexes
                    entity.HasIndex(e => new { e.ProjectId, e.DocumentType });
                    entity.HasIndex(e => new { e.ProjectId, e.IsLatest });
                    entity.HasIndex(e => e.FileHash);
              });

                  // ✅ PermitMetadata Configuration
                  modelBuilder.Entity<PermitMetadata>(entity =>
                  {
                        entity.HasKey(e => e.Id);

                        entity.Property(e => e.PermitNumber)
                        .IsRequired()
                        .HasMaxLength(100);

                        entity.Property(e => e.Area)
                        .HasColumnType("numeric(18,2)");

                        entity.Property(e => e.Address)
                        .HasMaxLength(500);

                        entity.Property(e => e.Owner)
                        .HasMaxLength(200);

                        // 1-1 relationship with ProjectDocument
                        entity.HasOne(e => e.ProjectDocument)
                        .WithOne() // hoặc .WithOne(pd => pd.PermitMetadata)
                        .HasForeignKey<PermitMetadata>(e => e.ProjectDocumentId)
                        .OnDelete(DeleteBehavior.Cascade);

                        entity.HasIndex(e => e.PermitNumber);
                        entity.HasIndex(e => e.ExpiryDate);
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
                  // ✅ Project configuration (UPDATED)
                  modelBuilder.Entity<Project>(entity =>
                  {
                        entity.HasKey(e => e.Id);

                        entity.Property(e => e.Name).IsRequired().HasMaxLength(200);

                        // Status là enum → lưu int
                        entity.Property(e => e.Status)
            .HasConversion<int>();              // <-- thay cho HasMaxLength(50)
                        entity.HasIndex(e => e.Status);

                        // SupervisorId OPTIONAL (SetNull khi supervisor bị xóa)
                        entity.HasOne(e => e.Supervisor)
            .WithMany()
            .HasForeignKey(e => e.SupervisorId)
            .OnDelete(DeleteBehavior.SetNull);   // <-- bỏ .IsRequired()

                        // Homeowner (khóa ngoại tới User), nên Restrict xóa
                        entity.HasOne(e => e.Homeowner)
      .WithMany()
      .HasForeignKey(e => e.HomeownerId)
      .OnDelete(DeleteBehavior.Restrict);

                        // (Optional) nếu bạn muốn cấu hình Contracts rõ ràng:
                        // entity.HasMany(p => p.Contracts)
                        //       .WithOne(c => c.Project)
                        //       .HasForeignKey(c => c.ProjectId)
                        //       .OnDelete(DeleteBehavior.Restrict);
                  });
                  // ✅ ProjectParticipant configuration (NEW)
                  modelBuilder.Entity<ProjectParticipant>(b =>
                  {
                        b.HasKey(pp => pp.Id);

                        // Enum → int
                        b.Property(pp => pp.Role).HasConversion<int>();
                        b.Property(pp => pp.Status).HasConversion<int>();

                        b.HasOne(pp => pp.Project)
       .WithMany(p => p.Participants)
       .HasForeignKey(pp => pp.ProjectId)
       .OnDelete(DeleteBehavior.Cascade);

                        b.HasOne(pp => pp.User)
       .WithMany()
       .HasForeignKey(pp => pp.UserId)
       .OnDelete(DeleteBehavior.Cascade);

                        // 1 user chỉ tham gia 1 lần trong 1 project
                        b.HasIndex(pp => new { pp.ProjectId, pp.UserId }).IsUnique();
                        b.HasIndex(pp => new { pp.ProjectId, pp.Role });
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

                  // ProgressMedia entity configuration
                  modelBuilder.Entity<ProgressMedia>(entity =>
                  {
                        entity.HasKey(e => e.Id);

                        entity.Property(e => e.Url)
                        .IsRequired()
                        .HasMaxLength(1000);

                        entity.Property(e => e.Caption)
                        .HasMaxLength(500);

                        entity.Property(e => e.FileName)
                        .IsRequired()
                        .HasMaxLength(255);

                        entity.Property(e => e.ContentType)
                        .IsRequired()
                        .HasMaxLength(100);

                        entity.HasOne(e => e.Project)
                        .WithMany()
                        .HasForeignKey(e => e.ProjectId)
                        .OnDelete(DeleteBehavior.Cascade);

                        entity.HasOne(e => e.Creator)
                        .WithMany()
                        .HasForeignKey(e => e.CreatorId)
                        .OnDelete(DeleteBehavior.Restrict);

                        entity.HasIndex(e => new { e.ProjectId, e.CreatedAt });
                  });
                  // QuoteRequest
                  modelBuilder.Entity<QuoteRequest>(e =>
                  {
                        e.HasKey(x => x.Id);
                        e.Property(x => x.Scope).HasMaxLength(2000);
                        e.Property(x => x.Status).HasConversion<int>(); // enum->int

                        e.HasOne(x => x.Project)
                   .WithMany() // hoặc .WithMany(p => p.QuoteRequests) nếu bạn muốn thêm collection
                   .HasForeignKey(x => x.ProjectId)
                   .OnDelete(DeleteBehavior.Cascade);

                        e.HasIndex(x => new { x.ProjectId, x.Status });
                  });

                  // ContractorPost configuration
                  modelBuilder.Entity<ContractorPost>(entity =>
                  {
                        entity.HasKey(e => e.Id);
                        entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                        entity.Property(e => e.Description).HasMaxLength(4000);
                        entity.HasOne(e => e.Contractor)
                        .WithMany()
                        .HasForeignKey(e => e.ContractorId)
                        .OnDelete(DeleteBehavior.Cascade);
                        entity.HasIndex(e => new { e.ContractorId, e.CreatedAt });
                  });

                  // ContractorPostImage configuration
                  modelBuilder.Entity<ContractorPostImage>(entity =>
                  {
                        entity.HasKey(e => e.Id);
                        entity.Property(e => e.Url).IsRequired().HasMaxLength(1000);
                        entity.Property(e => e.Caption).HasMaxLength(500);
                        entity.HasOne(e => e.ContractorPost)
                        .WithMany(p => p.Images)
                        .HasForeignKey(e => e.ContractorPostId)
                        .OnDelete(DeleteBehavior.Cascade);
                        entity.HasIndex(e => e.ContractorPostId);
                  });

                  // QuoteInvite
                  modelBuilder.Entity<QuoteInvite>(e =>
                  {
                        e.HasKey(x => x.Id);

                        e.HasOne(x => x.QuoteRequest)
                   .WithMany(q => q.Invites)
                   .HasForeignKey(x => x.QuoteRequestId)
                   .OnDelete(DeleteBehavior.Cascade);

                        // 1 contractor chỉ được mời 1 lần cho 1 quote
                        e.HasIndex(x => new { x.QuoteRequestId, x.ContractorUserId })
                   .IsUnique();
                  });
                  // Proposal
                  modelBuilder.Entity<Proposal>(e =>
                  {
                        e.HasKey(x => x.Id);

                        e.Property(x => x.Status).HasConversion<int>();
                        e.Property(x => x.PriceTotal).HasColumnType("numeric(18,2)");
                        e.Property(x => x.TermsSummary).HasMaxLength(2000);

                        e.HasOne(x => x.QuoteRequest)
                   .WithMany() // hoặc WithMany(q=>q.Proposals) nếu bạn muốn list ngược
                   .HasForeignKey(x => x.QuoteRequestId)
                   .OnDelete(DeleteBehavior.Cascade);

                        // Mỗi contractor chỉ được nộp 1 proposal cho 1 quote
                        e.HasIndex(x => new { x.QuoteRequestId, x.ContractorUserId })
                   .IsUnique();
                  });

                  // ProposalItem
                  modelBuilder.Entity<ProposalItem>(e =>
                  {
                        e.HasKey(x => x.Id);

                        e.Property(x => x.Name).HasMaxLength(300);
                        e.Property(x => x.Unit).HasMaxLength(50);
                        e.Property(x => x.Qty).HasColumnType("numeric(18,2)");
                        e.Property(x => x.UnitPrice).HasColumnType("numeric(18,2)");

                        e.HasOne(x => x.Proposal)
                   .WithMany(p => p.Items)
                   .HasForeignKey(x => x.ProposalId)
                   .OnDelete(DeleteBehavior.Cascade);
                  });

                  // ProjectDailyResource configuration
                  modelBuilder.Entity<ProjectDailyResource>(entity =>
                  {
                        entity.HasKey(e => e.Id);

                        // Foreign key to Project
                        entity.HasOne(e => e.Project)
                        .WithMany(p => p.DailyResources)
                        .HasForeignKey(e => e.ProjectId)
                        .OnDelete(DeleteBehavior.Cascade);

                        // Indexes for better performance
                        entity.HasIndex(e => new { e.ProjectId, e.ResourceDate });
                        entity.HasIndex(e => e.ResourceDate);

                        // Decimal precision for material quantities
                        entity.Property(e => e.CementConsumed)
                        .HasColumnType("decimal(18,2)");
                        entity.Property(e => e.CementRemaining)
                        .HasColumnType("decimal(18,2)");
                        entity.Property(e => e.SandConsumed)
                        .HasColumnType("decimal(18,2)");
                        entity.Property(e => e.SandRemaining)
                        .HasColumnType("decimal(18,2)");
                        entity.Property(e => e.AggregateConsumed)
                        .HasColumnType("decimal(18,2)");
                        entity.Property(e => e.AggregateRemaining)
                        .HasColumnType("decimal(18,2)");

                        // Notes field
                        entity.Property(e => e.Notes)
                        .HasMaxLength(1000);
                  });

                  // ProjectDailyResource configuration
                  modelBuilder.Entity<ProjectDailyResource>(entity =>
                  {
                        entity.HasKey(e => e.Id);

                        // Foreign key to Project
                        entity.HasOne(e => e.Project)
                        .WithMany(p => p.DailyResources)
                        .HasForeignKey(e => e.ProjectId)
                        .OnDelete(DeleteBehavior.Cascade);
                  });

                  // Wallet
                  modelBuilder.Entity<Wallet>(e =>
                  {
                        e.HasKey(x => x.Id);
                        e.Property(x => x.Available).HasColumnType("numeric(18,2)");
                        e.HasIndex(x => x.UserId).IsUnique();
                  });

                  // WalletTransaction
                  modelBuilder.Entity<WalletTransaction>(e =>
                  {
                        e.HasKey(x => x.Id);
                        e.Property(x => x.Amount).HasColumnType("numeric(18,2)");
                        e.Property(x => x.MomoOrderId).HasMaxLength(64);
                        e.Property(x => x.MomoRequestId).HasMaxLength(64);
                        e.Property(x => x.Status).HasMaxLength(50);
                        e.HasIndex(x => new { x.UserId, x.MomoOrderId, x.MomoRequestId }).IsUnique();
                  });

                  // LedgerEntry
                  modelBuilder.Entity<LedgerEntry>(e =>
                  {
                        e.HasKey(x => x.Id);
                        e.Property(x => x.Type).HasConversion<int>();
                        e.Property(x => x.Amount).HasColumnType("numeric(18,2)");
                        e.Property(x => x.RefId).HasMaxLength(100);
                        e.HasOne<Wallet>()
                   .WithMany()
                   .HasForeignKey(x => x.WalletId)
                   .OnDelete(DeleteBehavior.Cascade);
                        e.HasIndex(x => new { x.WalletId, x.CreatedAt });
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
