using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FitStreak.Core.Models.Workout;
using FitStreak.Core.Services;
using FitStreak.ViewModels.Base;
using FitStreak.Views;
using System.Collections.ObjectModel;

namespace FitStreak.ViewModels;

public partial class WorkoutsViewModel : BaseViewModel
{
    private readonly IWorkoutService _workoutService;

    [ObservableProperty]
    private ObservableCollection<Workout> _workouts = [];

    [ObservableProperty]
    private bool _isEmpty;

    public WorkoutsViewModel(IWorkoutService workoutService)
    {
        _workoutService = workoutService;
        Title = "Workouts";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        await RunSafeAsync(async () =>
        {
            var list = await _workoutService.GetAllWorkoutsAsync();
            Workouts = new ObservableCollection<Workout>(list);
            IsEmpty = list.Count == 0;
        });
    }

    [RelayCommand]
    public async Task DeleteWorkoutAsync(int id)
    {
        await RunSafeAsync(async () =>
        {
            await _workoutService.DeleteWorkoutAsync(id);
            await LoadAsync();
        });
    }

    [RelayCommand]
    public async Task EditWorkoutAsync(int id)
    {
        await Shell.Current.GoToAsync($"EditWorkout?workoutId={id}");
    }

    [RelayCommand]
    public async Task CreateWorkoutAsync()
    {
        await Shell.Current.GoToAsync(nameof(CreateWorkoutPage));
    }
}
