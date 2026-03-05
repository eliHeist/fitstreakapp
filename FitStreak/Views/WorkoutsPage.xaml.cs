using FitStreak.ViewModels;

namespace FitStreak.Views;

public partial class WorkoutsPage : ContentPage
{
    private readonly WorkoutsViewModel _viewModel;

    public WorkoutsPage(WorkoutsViewModel viewModel)
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
}
