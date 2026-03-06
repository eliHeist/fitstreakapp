using FitStreak.Core.Models.Workout;
using FitStreak.ViewModels;

namespace FitStreak.Views;

public partial class SchedulePage : ContentPage
{
    private readonly ScheduleViewModel _viewModel;

    public SchedulePage(ScheduleViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadCommand.Execute(null);
    }

    // Called from the "Move" button via ShowRescheduleCommand
    // Shows a date picker dialog then calls RescheduleCommand
    public async Task ShowReschedulePicker(WorkoutSchedule schedule)
    {
        var result = await DisplayPromptAsync(
            "Reschedule Workout",
            $"Enter new date for {schedule.Workout?.Name}",
            placeholder: "e.g. 2026-03-15");

        if (result is null) return;

        if (DateTime.TryParse(result, out DateTime newDate))
            await _viewModel.RescheduleCommand.ExecuteAsync((schedule, newDate));
        else
            await DisplayAlertAsync("Invalid Date", "Please enter a valid date.", "OK");
    }
}
