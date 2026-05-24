using academy_API.Models;
using Microsoft.EntityFrameworkCore;

namespace academy_API.Data;

public class TutoringDbContext : DbContext
{
    public TutoringDbContext(DbContextOptions<TutoringDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<PdpaConsent> PdpaConsents => Set<PdpaConsent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.Property(e => e.Id).HasColumnName("id");

            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.LineUserId);
            entity.HasIndex(e => e.Role);

            entity.Property(e => e.Email).HasMaxLength(255).HasColumnName("email");
            entity.Property(e => e.Phone).HasMaxLength(50).HasColumnName("phone");
            entity.Property(e => e.LineUserId).HasMaxLength(255).HasColumnName("line_user_id");
            entity.Property(e => e.PasswordHash).HasMaxLength(500).HasColumnName("password_hash");
            entity.Property(e => e.ResetToken).HasMaxLength(255).HasColumnName("reset_token");
            entity.Property(e => e.ResetTokenExpiry).HasColumnName("reset_token_expiry");
            entity.Property(e => e.Role).HasColumnName("role").HasConversion<string>();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.ToTable("students");
            entity.Property(s => s.Id).HasColumnName("id");
            entity.Property(s => s.UserId).HasColumnName("user_id");

            entity.HasOne(s => s.User)
                  .WithOne(u => u.Student)
                  .HasForeignKey<Student>(s => s.UserId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(s => s.UserId).IsUnique();
            entity.HasIndex(s => s.QrToken);

            entity.Property(s => s.FullName).HasMaxLength(255).HasColumnName("full_name");
            entity.Property(s => s.Nickname).HasMaxLength(100).HasColumnName("nickname");
            entity.Property(s => s.Grade).HasMaxLength(50).HasColumnName("grade");
            entity.Property(s => s.School).HasMaxLength(255).HasColumnName("school");
            entity.Property(s => s.QrToken).HasMaxLength(255).HasColumnName("qr_token");
            entity.Property(s => s.PhotoUrl).HasMaxLength(1000).HasColumnName("photo_url");
            entity.Property(s => s.MedicalInfo).HasMaxLength(2000).HasColumnName("medical_info");
            entity.Property(s => s.CreatedAt).HasColumnName("created_at");
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.ToTable("teachers");
            entity.Property(t => t.Id).HasColumnName("id");
            entity.Property(t => t.UserId).HasColumnName("user_id");

            entity.HasOne(t => t.User)
                  .WithOne(u => u.Teacher)
                  .HasForeignKey<Teacher>(t => t.UserId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(t => t.UserId).IsUnique();

            entity.Property(t => t.FullName).HasMaxLength(255).HasColumnName("full_name");
            entity.Property(t => t.Specialization).HasMaxLength(255).HasColumnName("subjects");
            entity.Property(t => t.Bio).HasMaxLength(2000).HasColumnName("bio");
            entity.Property(t => t.HourlyRate).HasColumnName("hourly_rate");
            entity.Property(t => t.PhotoUrl).HasMaxLength(255).HasColumnName("photo_url");

            entity.Ignore(t => t.Nickname);
            entity.Ignore(t => t.CreatedAt);
            entity.Ignore(t => t.UpdatedAt);
        });

        modelBuilder.Entity<PdpaConsent>(entity =>
        {
            entity.ToTable("pdpa_consents");
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasIndex(e => e.UserId);

            entity.Property(e => e.ConsentVersion).HasMaxLength(50).HasColumnName("consent_version");
            entity.Property(e => e.IsAccepted).HasColumnName("is_accepted");
            entity.Property(e => e.IpAddress).HasMaxLength(45).HasColumnName("ip_address");
            entity.Property(e => e.ConsentedAt).HasColumnName("accepted_at");
        });
    }
}
