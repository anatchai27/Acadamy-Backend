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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.LineUserId);
            entity.HasIndex(e => e.Role);

            entity.Property(e => e.Email).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.LineUserId).HasMaxLength(255);
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasOne(s => s.User)
                  .WithOne(u => u.Student)
                  .HasForeignKey<Student>(s => s.UserId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(s => s.UserId).IsUnique();
            entity.HasIndex(s => s.QrToken);

            entity.Property(s => s.FullName).HasMaxLength(255);
            entity.Property(s => s.Nickname).HasMaxLength(100);
            entity.Property(s => s.Grade).HasMaxLength(50);
            entity.Property(s => s.School).HasMaxLength(255);
            entity.Property(s => s.QrToken).HasMaxLength(255);
            entity.Property(s => s.PhotoUrl).HasMaxLength(1000);
            entity.Property(s => s.MedicalInfo).HasMaxLength(2000);
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.HasOne(t => t.User)
                  .WithOne(u => u.Teacher)
                  .HasForeignKey<Teacher>(t => t.UserId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(t => t.UserId).IsUnique();
            entity.HasIndex(t => t.Specialization);

            entity.Property(t => t.FullName).HasMaxLength(255);
            entity.Property(t => t.Nickname).HasMaxLength(100);
            entity.Property(t => t.Specialization).HasMaxLength(255);
            entity.Property(t => t.Bio).HasMaxLength(2000);
            entity.Property(t => t.PhotoUrl).HasMaxLength(1000);
        });
    }
}
