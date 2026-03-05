using FitStreak.ViewModels;

namespace FitStreak.Views;

[QueryProperty(nameof(WorkoutId), "workoutId")]
[QueryProperty(nameof(ScheduleId), "scheduleId")]
public partial class WorkoutRunnerPage : ContentPage
{
    private readonly WorkoutRunnerViewModel _viewModel;

    public string WorkoutId { get; set; } = string.Empty;
    public string ScheduleId { get; set; } = string.Empty;

    public WorkoutRunnerPage(WorkoutRunnerViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (int.TryParse(WorkoutId, out int workoutId) &&
            int.TryParse(ScheduleId, out int scheduleId))
        {
            await _viewModel.StartAsync(workoutId, scheduleId);
        }
    }

    protected override bool OnBackButtonPressed()
    {
        // Intercept Android back button — confirm abandon
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            bool abandon = await DisplayAlert(
                "Quit Workout?",
                "Your progress won't be saved.",
                "Quit", "Keep Going");

            if (abandon)
            {
                _viewModel.AbandonWorkoutCommand.Execute(null);
                await Shell.Current.GoToAsync("..");
            }
        });

        return true; // Prevent default back behaviour
    }
}
