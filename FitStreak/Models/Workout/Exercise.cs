using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitStreak.Models.Workout;

[Table("Exercises")]
public class Exercise
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    /// <summary>Foreign key to parent Workout</summary>
    [Required]
    public int WorkoutId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>Duration of the exercise in seconds e.g. 40</summary>
    [Range(1, 3600)]
    public int DurationSeconds { get; set; } = 30;

    /// <summary>Rest period immediately after this exercise in seconds e.g. 20</summary>
    [Range(0, 600)]
    public int RestAfterSeconds { get; set; } = 20;

    /// <summary>0-based position within the workout</summary>
    public int OrderIndex { get; set; }

    [MaxLength(300)]
    public string Notes { get; set; } = string.Empty;

    // Navigation property
    [ForeignKey(nameof(WorkoutId))]
    public Workout Workout { get; set; } = null!;
}