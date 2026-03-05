using FitStreak.ViewModels;
using Microsoft.Maui.Controls;

namespace FitStreak.Views;

public partial class StreakPage : ContentPage
{
    private readonly StreakViewModel _viewModel;
    private const int CellSize = 12;
    private const int CellSpacing = 3;

    public StreakPage(StreakViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadCommand.ExecuteAsync(null);
        BuildHeatmap();
    }

    private void BuildHeatmap()
    {
        HeatmapGrid.Children.Clear();
        HeatmapGrid.ColumnDefinitions.Clear();
        HeatmapGrid.RowDefinitions.Clear();

        var activity = _viewModel.YearlyActivity;
        var today = DateOnly.FromDateTime(DateTime.Today);
        var startDate = today.AddYears(-1).AddDays(1);

        // Align start to Monday
        while (startDate.DayOfWeek != DayOfWeek.Monday)
            startDate = startDate.AddDays(-1);

        // Count weeks needed
        int totalDays = today.DayNumber - startDate.DayNumber + 1;
        int totalWeeks = (int)Math.Ceiling(totalDays / 7.0);

        // Define columns (weeks) and rows (days Mon-Sun)
        for (int w = 0; w < totalWeeks; w++)
            HeatmapGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = CellSize + CellSpacing });

        for (int d = 0; d < 7; d++)
            HeatmapGrid.RowDefinitions.Add(new RowDefinition { Height = CellSize + CellSpacing });

        // Fill cells
        var current = startDate;
        for (int w = 0; w < totalWeeks; w++)
        {
            for (int d = 0; d < 7; d++)
            {
                if (current > today) break;

                activity.TryGetValue(current, out int count);

                var color = count switch
                {
                    0    => Color.FromArgb("#1E1E1E"),
                    1    => Color.FromArgb("#0E4429"),
                    2    => Color.FromArgb("#006D32"),
                    3    => Color.FromArgb("#26A641"),
                    >= 4 => Color.FromArgb("#39D353"),
                    _    => Color.FromArgb("#1E1E1E")
                };

                var capturedDate = current;

                var cell = new BoxView
                {
                    WidthRequest = CellSize,
                    HeightRequest = CellSize,
                    BackgroundColor = color,
                    CornerRadius = 2,
                    Margin = new Thickness(0, 0, CellSpacing, CellSpacing)
                };

                // Tap to select day
                var tap = new TapGestureRecognizer();
                tap.Tapped += (s, e) => _viewModel.SelectDayCommand.Execute(capturedDate);
                cell.GestureRecognizers.Add(tap);

                HeatmapGrid.SetColumn(cell, w);
                HeatmapGrid.SetRow(cell, d);
                HeatmapGrid.Children.Add(cell);

                current = current.AddDays(1);
            }
        }
    }
}
