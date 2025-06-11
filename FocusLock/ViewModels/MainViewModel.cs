using FocusLock.Views;
using FocusLock.Controls;
using System;
using System.ComponentModel;
using System.Windows.Input;

public class MainViewModel : INotifyPropertyChanged
{
    public ICommand SwitchToDashboardCommand { get; }
    public ICommand SwitchToFocusCommand { get; }
    public ICommand SwitchToTasksCommand { get; }
    public ICommand SwitchToSettingsCommand { get; }
    public ICommand SwitchToDistractionsCommand { get; }

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


    public MainViewModel()
    {
        SwitchToDashboardCommand = new RelayCommand(_ => SwitchToDashboard());
        SwitchToFocusCommand = new RelayCommand(_ => SwitchToFocus());
        SwitchToTasksCommand = new RelayCommand(_ => SwitchToTasks());
        SwitchToSettingsCommand = new RelayCommand(_ => SwitchToSettings());
        SwitchToDistractionsCommand = new RelayCommand(_ => SwitchToDistractions());

        // Default page
        SwitchToDashboard();
    }

    private void SwitchToDashboard()
    {
        CurrentPage = new DashboardView();
        CurrentPageName = "Dashboard";
    }

    private void SwitchToFocus()
    {
        CurrentPage = new FocusView();
        CurrentPageName = "Focus";
    }

    private void SwitchToTasks()
    {
        CurrentPage = new TasksView();
        CurrentPageName = "Tasks";
    }

    private void SwitchToSettings()
    {
        CurrentPage = new SettingsView();
        CurrentPageName = "Settings";
    }

    private void SwitchToDistractions()
    {
        CurrentPage = new DistractionsView();
        CurrentPageName = "Distractions";
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public class RelayCommand : ICommand
{
    private readonly Action<object> _execute;
    private readonly Predicate<object> _canExecute;

    public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;
    public void Execute(object parameter) => _execute(parameter);

    public event EventHandler CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }
}

