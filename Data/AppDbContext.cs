using BackendPBPI.Models.AuthModel;
using BackendPBPI.Models.NewsModel;
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

        }
    }
}