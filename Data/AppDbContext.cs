using BackendPBPI.Models.AuthModel;
using BackendPBPI.Models.EventsModel;
using BackendPBPI.Models.NewsModel;
using BackendPBPI.Models.RankingModel;
using BackendPBPI.Models.RoleModel;
using BackendPBPI.Models.UserModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace BackendPBPI.Data
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
            Log.Information("Database context initialized.");
        }

        public DbSet<UserModel> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<RoleModel> Roles { get; set; }
        public DbSet<RoleUserModel> RoleUsers { get; set; }
        public DbSet<NewsHDRModel> NewsHDR { get; set; }
        public DbSet<NewsDTLModel> NewsDTL { get; set; }
        public DbSet<EventsHDRModel> EventsHDR { get; set; }
        public DbSet<EventsDTLModel> EventsDTL { get; set; }
        public DbSet<EventsFTRModel> EventsFTR { get; set; }
        public DbSet<RankingHDRModel> RankingHDR { get; set; }
        public DbSet<RankingDTLModel> RankingDTL { get; set; }
        public DbSet<RankingFTRModel> RankingFTR { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User Configuration
            modelBuilder.Entity<UserModel>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.UserID);
                entity.HasIndex(e => e.Email).IsUnique();
                entity.HasIndex(e => e.UserName).IsUnique();
                entity.Property(e => e.Status).HasDefaultValue((byte)1);
            });

            // RefreshToken Configuration
            modelBuilder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable("RefreshTokens");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Token);
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Role Configuration
            modelBuilder.Entity<RoleModel>(entity =>
            {
                entity.ToTable("Roles");
                entity.HasKey(e => e.RoleID);
                entity.HasIndex(e => e.RoleName).IsUnique();
                entity.Property(e => e.RoleName).IsRequired().HasMaxLength(50);
            });

            // RoleUser Configuration (Many-to-Many)
            modelBuilder.Entity<RoleUserModel>(entity =>
            {
                entity.ToTable("RoleUsers");
                entity.HasKey(e => e.RoleUserID);

                // Composite unique index untuk mencegah duplicate role assignment
                entity.HasIndex(e => new { e.UserID, e.RoleID }).IsUnique();

                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserID)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Role)
                      .WithMany(r => r.RoleUsers)
                      .HasForeignKey(e => e.RoleID)
                      .OnDelete(DeleteBehavior.Cascade);

            });

            // Konfigurasi News relationships
            modelBuilder.Entity<NewsHDRModel>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserID)
                .OnDelete(DeleteBehavior.NoAction);

            //Relasi One To One
            modelBuilder.Entity<NewsDTLModel>()
                .HasIndex(d => d.NewsHDRID)
                .IsUnique();

            modelBuilder.Entity<NewsDTLModel>()
                .HasOne(d => d.NewsHeader)
                .WithOne(h => h.NewsDetail)
                .HasForeignKey<NewsDTLModel>(d => d.NewsHDRID)
                .OnDelete(DeleteBehavior.Cascade);

            // ============ RANKING CONFIGURATIONS ============

            // Konfigurasi RankingHDR dengan User
            modelBuilder.Entity<RankingHDRModel>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserID)
                .OnDelete(DeleteBehavior.NoAction); // Prevent cascade delete

            // Konfigurasi One-to-One: RankingHDR <-> RankingDTL
            modelBuilder.Entity<RankingDTLModel>()
                .HasIndex(d => d.RankingHDRID)
                .IsUnique(); // Ensure one-to-one relationship

            modelBuilder.Entity<RankingDTLModel>()
                .HasOne(d => d.RankingHeader)
                .WithOne(h => h.RankingDetail)
                .HasForeignKey<RankingDTLModel>(d => d.RankingHDRID)
                .OnDelete(DeleteBehavior.Cascade); // When HDR deleted, DTL also deleted

            // Konfigurasi One-to-Many: RankingHDR <-> RankingFTR
            modelBuilder.Entity<RankingFTRModel>()
                .HasOne(f => f.RankingHeader)
                .WithMany(h => h.RankingFooters)
                .HasForeignKey(f => f.RankingHDRID)
                .OnDelete(DeleteBehavior.Cascade); // When HDR deleted, all FTR also deleted

            // Optional: Index untuk performa query
            modelBuilder.Entity<RankingHDRModel>()
                .HasIndex(r => r.PlayerName); // Index for search by name

            modelBuilder.Entity<RankingHDRModel>()
                .HasIndex(r => r.PlayerPoints); // Index for ranking/sorting by points

            modelBuilder.Entity<RankingHDRModel>()
                .HasIndex(r => r.DeletedAt); // Index for filtering soft-deleted records

            modelBuilder.Entity<RankingFTRModel>()
                .HasIndex(f => f.MatchDate); // Index for sorting match history by date

        }
    }
}