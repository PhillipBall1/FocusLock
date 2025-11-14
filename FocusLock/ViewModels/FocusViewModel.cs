using FocusLock.Service;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;

namespace FocusLock.ViewModels
{
    public class FocusViewModel : BaseViewModel, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // Constructor hooks into task and focus events and loads initial state
        public FocusViewModel()
        {
            TaskService.TasksUpdated += OnTasksUpdated;
            SyncFromTaskService();

            FocusService.FocusModeChanged += (isActive) =>
            {
                // Notify UI on main thread when focus mode changes
                App.Current.Dispatcher.Invoke(() =>
                {
                    OnPropertyChanged(nameof(IsFocusActive));
                });
            };
        }

        // Notify property change (from BaseViewModel, but repeated here for explicitness)
        public void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));


        #region Task Info

        // Called when tasks update, refreshes properties on UI thread
        private void OnTasksUpdated()
        {
            App.Current.Dispatcher.Invoke(SyncFromTaskService);
        }

        // Indicates if the focus mode is currently active
        public bool IsFocusActive => FocusService.IsFocusModeActive;

        private string _upcomingTaskTitle = "Loading...";
        public string UpcomingTaskTitle
        {
            get => _upcomingTaskTitle;
            set { _upcomingTaskTitle = value; OnPropertyChanged(); }
        }

        // Static copies for quick external access if needed
        public static string taskTitleStatic;
        public static TimeSpan taskTimeStatic;

        private TimeSpan _upcomingTaskStartTime = TimeSpan.Zero;
        public TimeSpan UpcomingTaskStartTime
        {
            get => _upcomingTaskStartTime;
            set { _upcomingTaskStartTime = value; OnPropertyChanged(); }
        }

        private bool _isTaskRunning;
        public bool IsTaskRunning
        {
            get => _isTaskRunning;
            set
            {
                if (_isTaskRunning != value)
                {
                    _isTaskRunning = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(TaskHeader));
                    OnPropertyChanged(nameof(TaskHeaderColor));
                }
            }
        }

        // Header text depending on task running state
        public string TaskHeader => IsTaskRunning ? "Active Task" : "Upcoming Task";

        // Header color changes based on task running state (red if active, blue if upcoming)
        public Brush TaskHeaderColor => IsTaskRunning
            ? new SolidColorBrush(Color.FromRgb(198, 40, 40))  // Red
            : new SolidColorBrush(Color.FromRgb(21, 101, 192)); // Blue

        // Synchronize properties with the current tasks from TaskService
        private void SyncFromTaskService()
        {
            var activeTask = TaskService.ActiveTask;
            IsTaskRunning = activeTask != null;

            if (IsTaskRunning)
            {
                UpcomingTaskTitle = activeTask?.Title ?? "";
                UpcomingTaskStartTime = activeTask?.StartTime ?? TimeSpan.Zero;
                taskTitleStatic = activeTask?.Title ?? "";
                taskTimeStatic = activeTask?.StartTime ?? TimeSpan.Zero;
            }
            else
            {
                var nextTask = TaskService.NextTask;
                UpcomingTaskTitle = nextTask?.Title ?? "No upcoming tasks";
                UpcomingTaskStartTime = nextTask?.StartTime ?? TimeSpan.Zero;
                taskTitleStatic = nextTask?.Title ?? "No upcoming tasks";
                taskTimeStatic = nextTask?.StartTime ?? TimeSpan.Zero;
            }

            OnPropertyChanged(nameof(IsFocusActive));
            OnPropertyChanged(nameof(TaskHeader));
            OnPropertyChanged(nameof(TaskHeaderColor));
        }

        #endregion
    }
}
