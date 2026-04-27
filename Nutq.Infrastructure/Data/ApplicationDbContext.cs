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
        public DbSet<InvitationCode> InvitationCodes { get; set; } = null!;
        public DbSet<Exercise> Exercises { get; set; } = null!;
        public DbSet<DifficultyLevel> DifficultyLevels { get; set; } = null!;
        public DbSet<TherapyPlan> TherapyPlans { get; set; } = null!;
        public DbSet<PlanExercise> PlanExercises { get; set; } = null!;
        public DbSet<Vocabulary> Vocabularies { get; set; } = null!;
        public DbSet<WeeklyReport> WeeklyReports { get; set; } = null!;
        public DbSet<ExerciseProgress> ExerciseProgresses { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Example relationships
            modelBuilder.Entity<Doctor>()
                .HasMany(d => d.Patients)
                .WithOne(p => p.Doctor)
                .HasForeignKey(p => p.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Doctor>()
                .HasMany(d => d.TherapyPlans)
                .WithOne(tp => tp.Doctor)
                .HasForeignKey(tp => tp.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Patient>()
                .HasMany(p => p.TherapyPlans)
                .WithOne(tp => tp.Patient)
                .HasForeignKey(tp => tp.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TherapyPlan>()
                .HasMany(tp => tp.PlanExercises)
                .WithOne(pe => pe.TherapyPlan)
                .HasForeignKey(pe => pe.TherapyPlanId);

            modelBuilder.Entity<Exercise>()
                .HasMany(e => e.PlanExercises)
                .WithOne(pe => pe.Exercise)
                .HasForeignKey(pe => pe.ExerciseId);

            modelBuilder.Entity<PlanExercise>()
                .HasMany(pe => pe.ExerciseProgressRecords)
                .WithOne(ep => ep.PlanExercise)
                .HasForeignKey(ep => ep.PlanExerciseId);

            modelBuilder.Entity<Patient>()
                .HasMany(p => p.ExerciseProgressRecords)
                .WithOne(ep => ep.Patient)
                .HasForeignKey(ep => ep.PatientId);
        }
    }
}
