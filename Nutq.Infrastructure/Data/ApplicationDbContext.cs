using Microsoft.EntityFrameworkCore;
using Nutq.Core.Entities;

namespace Nutq.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // DbSets
        public DbSet<Doctor> Doctors { get; set; } = null!;
        public DbSet<Patient> Patients { get; set; } = null!;
        public DbSet<Admin> Admins { get; set; } = null!;
        public DbSet<InvitationCode> InvitationCodes { get; set; } = null!;
        public DbSet<Exercise> Exercises { get; set; } = null!;
        public DbSet<DifficultyLevel> DifficultyLevels { get; set; } = null!;
        public DbSet<TherapyPlan> TherapyPlans { get; set; } = null!;
        public DbSet<PlanExercise> PlanExercises { get; set; } = null!;
        public DbSet<Vocabulary> Vocabularies { get; set; } = null!;
        public DbSet<VocabularyExercise> VocabularyExercises { get; set; } = null!;
        public DbSet<WeeklyReport> WeeklyReports { get; set; } = null!;
        public DbSet<ExerciseProgress> ExerciseProgresses { get; set; } = null!;
        public DbSet<DoctorReview> DoctorReviews { get; set; } = null!;
        public DbSet<DoctorPatientRelationship> DoctorPatientRelationships { get; set; } = null!;
        public DbSet<TransferRequest> TransferRequests { get; set; } = null!;
        public DbSet<TrainingSession> TrainingSessions { get; set; } = null!;
        public DbSet<SpeechAttempt> SpeechAttempts { get; set; } = null!;
        public DbSet<ProgressSnapshot> ProgressSnapshots { get; set; } = null!;
        public DbSet<CategoryPerformanceSnapshot> CategoryPerformanceSnapshots { get; set; } = null!;
        public DbSet<SessionClinicalReport> SessionClinicalReports { get; set; } = null!;
        public DbSet<PronunciationPattern> PronunciationPatterns { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Doctor relationships
            modelBuilder.Entity<Doctor>()
                .HasMany(d => d.Patients)
                .WithOne(p => p.Doctor)
                .HasForeignKey(p => p.DoctorId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<DoctorPatientRelationship>()
                .HasOne(r => r.Doctor)
                .WithMany(d => d.PatientRelationships)
                .HasForeignKey(r => r.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DoctorPatientRelationship>()
                .HasOne(r => r.Patient)
                .WithMany(p => p.DoctorRelationships)
                .HasForeignKey(r => r.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TransferRequest>()
                .HasOne(t => t.Patient)
                .WithMany(p => p.TransferRequests)
                .HasForeignKey(t => t.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TransferRequest>()
                .HasOne(t => t.FromDoctor)
                .WithMany(d => d.OutgoingTransferRequests)
                .HasForeignKey(t => t.FromDoctorId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<TransferRequest>()
                .HasOne(t => t.ToDoctor)
                .WithMany(d => d.IncomingTransferRequests)
                .HasForeignKey(t => t.ToDoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Doctor>()
                .HasMany(d => d.TherapyPlans)
                .WithOne(tp => tp.Doctor)
                .HasForeignKey(tp => tp.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Doctor>()
                .HasMany(d => d.Reviews)
                .WithOne(r => r.Doctor)
                .HasForeignKey(r => r.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            // Patient relationships
            modelBuilder.Entity<Patient>()
                .HasMany(p => p.TherapyPlans)
                .WithOne(tp => tp.Patient)
                .HasForeignKey(tp => tp.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Patient>()
                .HasMany(p => p.DoctorReviews)
                .WithOne(r => r.Patient)
                .HasForeignKey(r => r.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            // TherapyPlan relationships
            modelBuilder.Entity<TherapyPlan>()
                .HasMany(tp => tp.PlanExercises)
                .WithOne(pe => pe.TherapyPlan)
                .HasForeignKey(pe => pe.TherapyPlanId);

            // Exercise relationships
            modelBuilder.Entity<Exercise>()
                .HasMany(e => e.PlanExercises)
                .WithOne(pe => pe.Exercise)
                .HasForeignKey(pe => pe.ExerciseId);

            // PlanExercise relationships
            modelBuilder.Entity<PlanExercise>()
                .HasMany(pe => pe.ExerciseProgressRecords)
                .WithOne(ep => ep.PlanExercise)
                .HasForeignKey(ep => ep.PlanExerciseId);

            // ExerciseProgress relationships
            modelBuilder.Entity<Patient>()
                .HasMany(p => p.ExerciseProgressRecords)
                .WithOne(ep => ep.Patient)
                .HasForeignKey(ep => ep.PatientId);

            // VocabularyExercise relationships
            modelBuilder.Entity<VocabularyExercise>()
                .HasOne(ve => ve.Vocabulary)
                .WithMany()
                .HasForeignKey(ve => ve.VocabularyId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VocabularyExercise>()
                .HasOne(ve => ve.Exercise)
                .WithMany()
                .HasForeignKey(ve => ve.ExerciseId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VocabularyExercise>()
                .HasOne(ve => ve.DifficultyLevel)
                .WithMany()
                .HasForeignKey(ve => ve.DifficultyLevelId)
                .OnDelete(DeleteBehavior.Cascade);

            // Admin relationships
            modelBuilder.Entity<Admin>()
                .HasMany(a => a.InvitationCodes)
                .WithOne(ic => ic.Admin)
                .HasForeignKey(ic => ic.AdminId)
                .OnDelete(DeleteBehavior.SetNull);

            // InvitationCode relationships
            modelBuilder.Entity<InvitationCode>()
                .HasOne(ic => ic.Doctor)
                .WithMany(d => d.InvitationCodes)
                .HasForeignKey(ic => ic.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            // DoctorReview constraints
            modelBuilder.Entity<DoctorReview>()
                .HasIndex(dr => new { dr.DoctorId, dr.PatientId })
                .IsUnique();

            modelBuilder.Entity<TrainingSession>()
                .HasOne(ts => ts.ExerciseProgress)
                .WithOne()
                .HasForeignKey<TrainingSession>(ts => ts.ExerciseProgressId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TrainingSession>()
                .HasIndex(ts => ts.ExerciseProgressId)
                .IsUnique();

            modelBuilder.Entity<ProgressSnapshot>()
                .HasOne(ps => ps.TrainingSession)
                .WithOne(ts => ts.ProgressSnapshot)
                .HasForeignKey<ProgressSnapshot>(ps => ps.TrainingSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProgressSnapshot>()
                .HasIndex(ps => ps.TrainingSessionId)
                .IsUnique();

            modelBuilder.Entity<SessionClinicalReport>()
                .HasOne(r => r.TrainingSession)
                .WithOne(ts => ts.ClinicalReport)
                .HasForeignKey<SessionClinicalReport>(r => r.TrainingSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SessionClinicalReport>()
                .HasIndex(r => r.TrainingSessionId)
                .IsUnique();

            modelBuilder.Entity<CategoryPerformanceSnapshot>()
                .HasOne(c => c.ProgressSnapshot)
                .WithMany(ps => ps.CategoryPerformances)
                .HasForeignKey(c => c.ProgressSnapshotId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SpeechAttempt>()
                .HasOne(a => a.TrainingSession)
                .WithMany(ts => ts.SpeechAttempts)
                .HasForeignKey(a => a.TrainingSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<PronunciationPattern>()
                .HasIndex(p => new { p.PatientId, p.ExpectedPattern, p.RecognizedPattern, p.PatternType })
                .IsUnique();

            modelBuilder.Entity<SpeechAttempt>()
                .HasIndex(a => new { a.PatientId, a.AttemptedAt });

            modelBuilder.Entity<ProgressSnapshot>()
                .HasIndex(p => new { p.PatientId, p.SnapshotDate });

            modelBuilder.Entity<CategoryPerformanceSnapshot>()
                .HasIndex(c => new { c.PatientId, c.Category });

            modelBuilder.Entity<PronunciationPattern>()
                .HasIndex(p => new { p.PatientId, p.IsActive });
        }
    }
}
