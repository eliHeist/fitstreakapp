using FitStreak.ViewModels;

namespace FitStreak.Views;

[QueryProperty(nameof(WorkoutId), "workoutId")]
public partial class CreateWorkoutPage : ContentPage
{
    private readonly CreateWorkoutViewModel _viewModel;

    public string WorkoutId
    {
        set
        {
            if (int.TryParse(value, out int id) && id > 0)
                _viewModel.LoadForEditCommand.Execute(id);
        }
    }

    public CreateWorkoutPage(CreateWorkoutViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }
}
