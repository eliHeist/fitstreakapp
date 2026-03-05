using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitStreak.Core.Models.Workout;

[Table("WorkoutSessions")]
public class WorkoutSession
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>Foreign key to the schedule entry this session belongs to</summary>
    [Required]
    public int WorkoutScheduleId { get; set; }

    /// <summary>When the user tapped Start Workout</summary>
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;

    /// <summary>When the user tapped Complete — null if abandoned</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>True only if user reached the end and tapped Complete</summary>
    public bool IsCompleted { get; set; } = false;

    // Navigation property
    [ForeignKey(nameof(WorkoutScheduleId))]
    public WorkoutSchedule Schedule { get; set; } = null!;

    // Computed — not mapped to DB
    [NotMapped]
    public TimeSpan? Duration => CompletedAt.HasValue
        ? CompletedAt.Value - StartedAt
        : null;

    [NotMapped]
    public bool WasAbandoned => !IsCompleted && CompletedAt == null;
}