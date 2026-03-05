using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitStreak.Core.Models.Workout;

public enum RecurrenceType
{
    None,
    Weekly,
    Monthly,
    Yearly
}

public enum ScheduleStatus
{
    Pending,
    Completed,
    Missed,
    Rescheduled
}

[Table("WorkoutSchedules")]
public class WorkoutSchedule
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>Foreign key to the Workout being scheduled</summary>
    [Required]
    public int WorkoutId { get; set; }

    /// <summary>The date this workout is scheduled to be done</summary>
    [Required]
    public DateTime ScheduledDate { get; set; }

    /// <summary>How often this schedule repeats</summary>
    public RecurrenceType RecurrenceType { get; set; } = RecurrenceType.None;

    /// <summary>Current state of this scheduled workout</summary>
    public ScheduleStatus Status { get; set; } = ScheduleStatus.Pending;

    /// <summary>Populated when the user marks it complete</summary>
    public DateTime? CompletedAt { get; set; }

    /// <summary>
    /// Stores the original scheduled date if this entry was rescheduled.
    /// Null if never rescheduled.
    /// </summary>
    public DateTime? OriginalScheduledDate { get; set; }

    // Navigation properties
    [ForeignKey(nameof(WorkoutId))]
    public Workout Workout { get; set; } = null!;

    public ICollection<WorkoutSession> Sessions { get; set; } = [];

    // Computed helpers — not mapped to DB
    [NotMapped]
    public bool IsToday => ScheduledDate.Date == DateTime.Today;

    [NotMapped]
    public bool IsMissed =>
        Status == ScheduleStatus.Missed ||
        (Status == ScheduleStatus.Pending && ScheduledDate.Date < DateTime.Today);

    [NotMapped]
    public bool IsCompleted => Status == ScheduleStatus.Completed;
}