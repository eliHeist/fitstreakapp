# FitStreak — Copilot Workspace Instructions

## Project Overview

FitStreak is a **.NET MAUI 10** cross-platform fitness tracking app (Android primary, Windows supported) that lets users create workouts, schedule them with recurrence, run them with a live timer, and track streaks via a GitHub-style yearly heatmap.

**Solution structure:**
- `FitStreak.Core/` — class library (net10.0): EF Core models, DbContext, all service interfaces & implementations
- `FitStreak/` — MAUI app (net10.0-android, net10.0-windows10.0.19041.0): ViewModels, Views, converters, platform services

---

## Build & Run

```bash
# Build all projects
dotnet build

# Build Android target specifically
dotnet build -f net10.0-android

# Publish Android APK (Release)
dotnet publish -c Release -f net10.0-android

# EF Core migrations (run from solution root)
dotnet ef migrations add <MigrationName> --project FitStreak.Core
dotnet ef database update --project FitStreak.Core
```

> The app runs `Database.MigrateAsync()` on startup (`App.xaml.cs`). Before shipping to production, replace with explicit migration deployment.

---

## Architecture

### Layer Responsibilities

| Layer | Project | Responsibility |
|-------|---------|---------------|
| **Models** | `FitStreak.Core/Models/Workout/` | EF Core entities; `[NotMapped]` computed properties for client-side logic |
| **Data** | `FitStreak.Core/Data/` | `AppDbContext` (Fluent API config), `AppDbContextFactory` (design-time) |
| **Services** | `FitStreak.Core/Services/` | Business logic; interfaces + implementations; all async, all injected |
| **ViewModels** | `FitStreak/ViewModels/` | CommunityToolkit.Mvvm `ObservableObject` with `[ObservableProperty]` / `[RelayCommand]` |
| **Views** | `FitStreak/Views/` | XAML pages; code-behind limited to navigation and lifecycle hooks |
| **Converters** | `FitStreak/Converters/` | `IValueConverter` implementations used in XAML bindings |

### Key Data Models

- **Workout** → has many **Exercise** (ordered via `OrderIndex`, cascade delete)
- **WorkoutSchedule** → scheduled instance of a Workout; has `RecurrenceType` (None/Weekly/Monthly/Yearly) and `ScheduleStatus` (Pending/Completed/Missed/Rescheduled)
- **WorkoutSession** → actual run record linked to a `WorkoutSchedule`

### DI Registration (`MauiProgram.cs`)

- **Singleton**: `AppDbContext`, all `I*Service` implementations
- **Transient**: all ViewModels and Pages (fresh instance per navigation)

---

## Conventions

### MVVM Pattern

- All ViewModels inherit `BaseViewModel` from `FitStreak/ViewModels/Base/BaseViewModel.cs`
- **Always** wrap async commands in `RunSafeAsync()` — it manages `IsBusy`, `HasError`, and `ErrorMessage` automatically
- Use `[ObservableProperty]` for bindable properties; use `[RelayCommand]` for commands
- `IsBusy` / `IsNotBusy` prevents double-tap; bind buttons to `IsNotBusy`
- `ErrorMessage` / `HasError` are surfaced to the UI via binding — do not show error dialogs directly

```csharp
[RelayCommand]
private Task LoadAsync() => RunSafeAsync(async () =>
{
    var items = await _service.GetAllAsync();
    Items = new ObservableCollection<Item>(items);
});
```

### Services

- All service methods are `async Task<T>` — no sync DB calls
- DateTime stored as UTC in the database; convert to local time only at the UI/ViewModel layer
- Use `DateOnly` for streak/calendar calculations to avoid timezone edge cases
- `ScheduleService` is responsible for auto-marking stale Pending schedules as Missed on startup

### Data / EF Core

- All entity configuration is done via Fluent API in `OnModelCreating()` — do not use data annotations for constraints
- Add `HasIndex()` for any column used in frequent `Where` queries
- `[NotMapped]` for computed properties that should not be persisted (e.g., `IsToday`, `Duration`, `WasAbandoned`)
- DB file location: `FileSystem.AppDataDirectory/fitstreak.db`

### Naming

- Interfaces: `I` prefix (e.g., `IWorkoutService`)
- ViewModels: `*ViewModel` suffix
- Pages: `*Page` suffix (XAML + code-behind)
- Converters: descriptive name + `Converter` suffix (e.g., `StatusToColorConverter`)
- Use XML doc comments (`///`) on all public service and interface members

### Error Handling

- `RunSafeAsync()` is the single entry point for all async ViewModel operations
- Do not swallow exceptions silently — always propagate or surface via `SetError()`
- Platform-specific code lives in `Platforms/` folders, never scattered in shared code

---

## Features & Pages

| Tab | ViewModel | Purpose |
|-----|-----------|---------|
| **Today** | `HomeViewModel` | Today's scheduled workouts, missed workouts, current streak count |
| **Workouts** | `WorkoutsViewModel` / `CreateWorkoutViewModel` | Workout library CRUD; exercise list with drag-to-reorder |
| **Schedule** | `ScheduleViewModel` | Calendar view; schedule workouts with recurrence |
| **Streaks** | `StreakViewModel` | Yearly activity heatmap, current streak, longest streak, total completions |
| *(Modal)* | `WorkoutRunnerViewModel` | Live timer for exercises + rest periods; pause/skip/complete |

---

## Common Pitfalls

- **Do not** skip `RunSafeAsync` — raw `async void` commands will crash silently on unhandled exceptions
- **Do not** register ViewModels or Pages as Singletons — they must be Transient to avoid stale state across navigations
- **Do not** put business logic in code-behind (`.xaml.cs`) — keep it in ViewModels or Services
- **Do not** store local `DateTime.Now` in the database — always use `DateTime.UtcNow`
- The `Habits/` models directory is intentionally empty (reserved for a future feature)
- iOS/macOS targets are currently commented out in `FitStreak.csproj` — do not assume they build

## Known Bugs & Workarounds

- **ObjectDisposedException on app destroy**: MAUI bug in `ShellFragmentContainer.OnDestroy`. 
  Workaround in `MainActivity.cs` — wrap `base.OnDestroy()` in try/catch for `ObjectDisposedException`.
- **IDispatcherTimer on MAUI**: Always create via `Application.Current!.Dispatcher.CreateTimer()`. 
  Do not use `System.Timers.Timer` — it won't marshal to the UI thread.
- **StaticResource xmlns declarations**: Always declare `xmlns:*` namespaces on the root 
  `ContentPage` element, never inline on child elements.

## Theme Tokens

- Background: `#0A0A0A`
- Surface/Cards: `#1A1A1A`
- Border: `#2A2A2A`
- Accent green: `#39D353`
- Accent magenta: `#FF006E`
- Tab icons selected: `#FF6B35`, unselected: `#666666`
- Destructive: `#F44336`
- Muted text: `#555555`, secondary text: `#888888`

## Navigation Routes

- `//WorkoutsPage` — Workouts tab (root)
- `//TodayPage` — Today tab (root)  
- `WorkoutRunnerPage?workoutId={id}&scheduleId={id}` — modal runner
- `CreateWorkoutPage` — pushed onto Workouts tab stack

## Business Rules

- A Workout cannot be saved without at least one Exercise
- WorkoutRunner auto-advances: Exercise → Rest → Next Exercise → ... → Complete
- Timer starts with a 5 second delay after `OnAppearing`
- Pausing stops tick processing but keeps timer alive (IsPaused flag)
- Abandoning a workout does NOT write to the database
