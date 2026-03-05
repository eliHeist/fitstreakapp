using Microsoft.EntityFrameworkCore;
using FitStreak.Core.Models.Workout;

namespace FitStreak.Core.Data;

public class AppDbContext : DbContext
{
    // -------------------------------------------------------------------------
    // DbSets — one per entity, EF maps these to DB tables
    // -------------------------------------------------------------------------
    public DbSet<Workout> Workouts => Set<Workout>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<WorkoutSchedule> WorkoutSchedules => Set<WorkoutSchedule>();
    public DbSet<WorkoutSession> WorkoutSessions => Set<WorkoutSession>();

    // -------------------------------------------------------------------------
    // Constructor — receives DbContextOptions from DI (configured in MauiProgram)
    // -------------------------------------------------------------------------
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // -------------------------------------------------------------------------
    // Model configuration via Fluent API
    // Fluent API always wins over data annotations when both exist
    // -------------------------------------------------------------------------
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureWorkout(modelBuilder);
        ConfigureExercise(modelBuilder);
        ConfigureWorkoutSchedule(modelBuilder);
        ConfigureWorkoutSession(modelBuilder);
    }

    // -------------------------------------------------------------------------
    // Workout configuration
    // -------------------------------------------------------------------------
    private static void ConfigureWorkout(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Workout>(entity =>
        {
            entity.HasKey(w => w.Id);

            entity.Property(w => w.Name)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(w => w.Description)
                  .HasMaxLength(500);

            entity.Property(w => w.CreatedAt)
                  .IsRequired()
                  .HasDefaultValueSql("datetime('now')"); // SQLite UTC now

            // Index on Name for faster lookups in the workout library
            entity.HasIndex(w => w.Name);
        });
    }

    // -------------------------------------------------------------------------
    // Exercise configuration
    // -------------------------------------------------------------------------
    private static void ConfigureExercise(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Exercise>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property(e => e.DurationSeconds)
                  .IsRequired()
                  .HasDefaultValue(30);

            entity.Property(e => e.RestAfterSeconds)
                  .IsRequired()
                  .HasDefaultValue(20);

            entity.Property(e => e.OrderIndex)
                  .IsRequired()
                  .HasDefaultValue(0);

            entity.Property(e => e.Notes)
                  .HasMaxLength(300);

            // Relationship: Exercise belongs to one Workout
            // If Workout is deleted → all its Exercises are deleted too
            entity.HasOne(e => e.Workout)
                  .WithMany(w => w.Exercises)
                  .HasForeignKey(e => e.WorkoutId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Index for fast loading of exercises by workout
            entity.HasIndex(e => e.WorkoutId);

            // Composite index: load exercises in order efficiently
            entity.HasIndex(e => new { e.WorkoutId, e.OrderIndex });
        });
    }

    // -------------------------------------------------------------------------
    // WorkoutSchedule configuration
    // -------------------------------------------------------------------------
    private static void ConfigureWorkoutSchedule(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkoutSchedule>(entity =>
        {
            entity.HasKey(ws => ws.Id);

            entity.Property(ws => ws.ScheduledDate)
                  .IsRequired();

            // Store enums as strings in DB — readable if you ever inspect the DB file
            entity.Property(ws => ws.RecurrenceType)
                  .HasConversion<string>()
                  .HasMaxLength(20);

            entity.Property(ws => ws.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20);

            entity.Property(ws => ws.CompletedAt)
                  .IsRequired(false);

            entity.Property(ws => ws.OriginalScheduledDate)
                  .IsRequired(false);

            // Relationship: Schedule belongs to one Workout
            // If Workout is deleted → all its Schedules are deleted too
            entity.HasOne(ws => ws.Workout)
                  .WithMany(w => w.Schedules)
                  .HasForeignKey(ws => ws.WorkoutId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Index for fast "what's scheduled today" home page query
            entity.HasIndex(ws => ws.ScheduledDate);

            // Index for loading all schedules for a workout
            entity.HasIndex(ws => ws.WorkoutId);

            // Index for missed workout detection (Status + Date)
            entity.HasIndex(ws => new { ws.Status, ws.ScheduledDate });
        });
    }

    // -------------------------------------------------------------------------
    // WorkoutSession configuration
    // -------------------------------------------------------------------------
    private static void ConfigureWorkoutSession(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkoutSession>(entity =>
        {
            entity.HasKey(ws => ws.Id);

            entity.Property(ws => ws.StartedAt)
                  .IsRequired()
                  .HasDefaultValueSql("datetime('now')");

            entity.Property(ws => ws.CompletedAt)
                  .IsRequired(false);

            entity.Property(ws => ws.IsCompleted)
                  .IsRequired()
                  .HasDefaultValue(false);

            // Relationship: Session belongs to one Schedule
            // If Schedule is deleted → all its Sessions are deleted too
            entity.HasOne(ws => ws.Schedule)
                  .WithMany(s => s.Sessions)
                  .HasForeignKey(ws => ws.WorkoutScheduleId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Index for loading sessions by schedule
            entity.HasIndex(ws => ws.WorkoutScheduleId);

            // Index for streak map — query completions by date range
            entity.HasIndex(ws => new { ws.IsCompleted, ws.CompletedAt });
        });
    }
}