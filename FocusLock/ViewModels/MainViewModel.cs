using FocusLock.Views;
using FocusLock.Controls;
using System;
using System.ComponentModel;
using System.Windows.Input;

public class MainViewModel : INotifyPropertyChanged
{
    // Commands for switching views
    public ICommand SwitchToDashboardCommand { get; }
    public ICommand SwitchToFocusCommand { get; }
    public ICommand SwitchToTasksCommand { get; }
    public ICommand SwitchToSettingsCommand { get; }
    public ICommand SwitchToDistractionsCommand { get; }

    // Current displayed page (View)
    private object _currentPage;
    public object CurrentPage
    {
        get => _currentPage;
        set
        {
            _currentPage = value;
            OnPropertyChanged(nameof(CurrentPage));
        }
    }

    // Name of the current page for UI display (e.g., header)
    private string _currentPageName;
    public string CurrentPageName
    {
        get => _currentPageName;
        set
        {
            if (_currentPageName != value)
            {
                _currentPageName = value;
                OnPropertyChanged(nameof(CurrentPageName));
            }
        }
    }

    // Constructor initializes commands and sets default page
    public MainViewModel()
    {
        SwitchToDashboardCommand = new RelayCommand(_ => SwitchToDashboard());
        SwitchToFocusCommand = new RelayCommand(_ => SwitchToFocus());
        SwitchToTasksCommand = new RelayCommand(_ => SwitchToTasks());
        SwitchToSettingsCommand = new RelayCommand(_ => SwitchToSettings());
        SwitchToDistractionsCommand = new RelayCommand(_ => SwitchToDistractions());

        // Set default page on startup
        SwitchToDashboard();
    }

    // Switch to Dashboard view and update page name
    private void SwitchToDashboard()
    {
        CurrentPage = new DashboardView();
        CurrentPageName = "Dashboard";
    }

    // Switch to Focus view and update page name
    private void SwitchToFocus()
    {
        CurrentPage = new FocusView();
        CurrentPageName = "Focus";
    }

    // Switch to Tasks view and update page name
    private void SwitchToTasks()
    {
        CurrentPage = new TasksView();
        CurrentPageName = "Tasks";
    }

    // Switch to Settings view and update page name
    private void SwitchToSettings()
    {
        CurrentPage = new SettingsView();
        CurrentPageName = "Settings";
    }

    // Switch to Distractions view and update page name
    private void SwitchToDistractions()
    {
        CurrentPage = new DistractionsView();
        CurrentPageName = "Distractions";
    }

    // Notify UI of property changes
    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

// Basic ICommand implementation for MVVM command binding
public class RelayCommand : ICommand
{
    private readonly Action<object> _execute;
    private readonly Predicate<object>? _canExecute;

    // Initialize with execute action and optional canExecute predicate
    public RelayCommand(Action<object> execute, Predicate<object>? canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    // Check if command can execute
    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter!) ?? true;

    // Execute command action
    public void Execute(object? parameter) => _execute(parameter!);

    // Event to requery if command can execute
    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value!;
        remove => CommandManager.RequerySuggested -= value!;
    }
}
