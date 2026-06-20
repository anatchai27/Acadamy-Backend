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
    public DbSet<Parent> Parents => Set<Parent>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Attendance> Attendances => Set<Attendance>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Payment> Payments => Set<Payment>();

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
            entity.Property(s => s.Id).HasColumnName("id").ValueGeneratedOnAdd();
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
            entity.Property(s => s.QrTokenExpiry).HasColumnName("qr_token_expiry");
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
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.UserId).HasColumnName("user_id").IsRequired(false);
            entity.Property(e => e.StudentId).HasColumnName("student_id").IsRequired(false);

            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Student)
                  .WithMany()
                  .HasForeignKey(e => e.StudentId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.StudentId);

            entity.Property(e => e.ConsentVersion).HasMaxLength(50).HasColumnName("consent_version");
            entity.Property(e => e.IsAccepted).HasColumnName("is_accepted");
            entity.Property(e => e.IpAddress).HasMaxLength(45).HasColumnName("ip_address");
            entity.Property(e => e.ConsentedAt).HasColumnName("accepted_at");
        });
        modelBuilder.Entity<Parent>(entity =>
        {
            entity.ToTable("parents");
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.StudentId).HasColumnName("student_id");

            entity.HasOne(e => e.Student)
                  .WithMany(s => s.Parents)
                  .HasForeignKey(e => e.StudentId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.StudentId);

            entity.Property(e => e.FullName).HasMaxLength(255).HasColumnName("full_name");
            entity.Property(e => e.Phone).HasMaxLength(50).HasColumnName("phone");
            entity.Property(e => e.Relationship).HasMaxLength(100).HasColumnName("relationship");
            entity.Property(e => e.LineUserId).HasMaxLength(255).HasColumnName("line_user_id");
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.ToTable("sessions");
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.Name).HasMaxLength(255).HasColumnName("name");
            entity.Property(e => e.StartTime).HasColumnName("start_time");
            entity.Property(e => e.EndTime).HasColumnName("end_time");
            entity.Property(e => e.IsActive).HasColumnName("is_active");

            entity.HasMany(e => e.Attendances)
                  .WithOne()
                  .HasForeignKey("session_id")
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.ToTable("attendances");
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.SessionId).HasColumnName("session_id").IsRequired(false);
            entity.Property(e => e.Status).HasMaxLength(20).HasColumnName("status");
            entity.Property(e => e.Note).HasMaxLength(500).HasColumnName("note");
            entity.Property(e => e.CheckinAt).HasColumnName("checkin_at");
            entity.Property(e => e.CheckoutAt).HasColumnName("checkout_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.Student)
                  .WithMany()
                  .HasForeignKey(e => e.StudentId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.StudentId, e.SessionId });
            entity.HasIndex(e => e.StudentId);
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.ToTable("enrollments");
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.StudentId).HasColumnName("student_id");
            entity.Property(e => e.CourseId).HasColumnName("course_id");
            entity.Property(e => e.SessionsRemaining).HasColumnName("sessions_remaining");
            entity.Property(e => e.PaidAmount).HasColumnName("paid_amount").HasColumnType("decimal(10,2)");
            entity.Property(e => e.Status).HasMaxLength(20).HasColumnName("status");
            entity.Property(e => e.ExpiresAt).HasColumnName("expires_at");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.Student)
                  .WithMany()
                  .HasForeignKey(e => e.StudentId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Course)
                  .WithMany()
                  .HasForeignKey(e => e.CourseId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.StudentId);
            entity.HasIndex(e => e.CourseId);
            entity.HasIndex(e => new { e.StudentId, e.CourseId }).IsUnique();
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.ToTable("courses");
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.Name).HasMaxLength(255).HasColumnName("name");
            entity.Property(e => e.Subject).HasMaxLength(255).HasColumnName("subject");
            entity.Property(e => e.TotalSessions).HasColumnName("total_sessions");
            entity.Property(e => e.Price).HasColumnName("price").HasColumnType("decimal(10,2)");
            entity.Property(e => e.TeacherId).HasColumnName("teacher_id").IsRequired(false);

            entity.HasOne(e => e.Teacher)
                  .WithMany()
                  .HasForeignKey(e => e.TeacherId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => e.TeacherId);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("payments");
            entity.Property(e => e.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(e => e.EnrollmentId).HasColumnName("enrollment_id");
            entity.Property(e => e.InvoiceNo).HasMaxLength(50).HasColumnName("invoice_no");
            entity.Property(e => e.Amount).HasColumnName("amount").HasColumnType("decimal(10,2)");
            entity.Property(e => e.Method).HasMaxLength(20).HasColumnName("method");
            entity.Property(e => e.SlipUrl).HasMaxLength(1000).HasColumnName("slip_url");
            entity.Property(e => e.Note).HasMaxLength(500).HasColumnName("note");
            entity.Property(e => e.ReceiptPdfUrl).HasMaxLength(1000).HasColumnName("receipt_pdf_url");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(e => e.Enrollment)
                  .WithMany()
                  .HasForeignKey(e => e.EnrollmentId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.InvoiceNo).IsUnique();
            entity.HasIndex(e => e.EnrollmentId);
        });
    }
}
