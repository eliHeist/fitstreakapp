using FitStreak.Views;

namespace FitStreak;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        RegisterRoutes();
    }

    private static void RegisterRoutes()
    {
        // -------------------------------------------------------------------------
        // These pages are not in the tab bar — they are pushed onto the nav stack
        // or shown as modals. Register them here so Shell.GoToAsync() can find them.
        // -------------------------------------------------------------------------

        // Pushed from WorkoutsPage — create a new workout
        Routing.RegisterRoute(nameof(CreateWorkoutPage), typeof(CreateWorkoutPage));

        // Pushed from WorkoutsPage — edit an existing workout (same page, different mode)
        Routing.RegisterRoute("EditWorkout", typeof(CreateWorkoutPage));

        // Pushed from HomePage or SchedulePage — run through a workout
        Routing.RegisterRoute(nameof(WorkoutRunnerPage), typeof(WorkoutRunnerPage));
    }
}