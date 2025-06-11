using FocusLock.Helper;
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

        public bool IsFocusActive => FocusService.IsFocusModeActive;

        private string _upcomingTaskTitle = "Loading...";
        public string UpcomingTaskTitle
        {
            get => _upcomingTaskTitle;
            set { _upcomingTaskTitle = value; OnPropertyChanged(); }
        }

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

        public string TaskHeader => IsTaskRunning ? "Active Task" : "Upcoming Task";

        public Brush TaskHeaderColor => IsTaskRunning
            ? new SolidColorBrush(Color.FromRgb(198, 40, 40))
            : new SolidColorBrush(Color.FromRgb(21, 101, 192));

        public FocusViewModel()
        {
            TaskService.TasksUpdated += OnTasksUpdated;
            SyncFromTaskService();
            FocusService.FocusModeChanged += (isActive) =>
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    OnPropertyChanged(nameof(IsFocusActive));
                });
            };
        }

        private void OnTasksUpdated()
        {
            App.Current.Dispatcher.Invoke(SyncFromTaskService);
        }

        private void SyncFromTaskService()
        {
            var activeTask = TaskService.ActiveTask;
            IsTaskRunning = activeTask != null;

            if (IsTaskRunning)
            {
                UpcomingTaskTitle = activeTask?.Title ?? "";
                UpcomingTaskStartTime = activeTask?.StartTime ?? TimeSpan.Zero;
            }
            else
            {
                var nextTask = TaskService.NextTask;
                UpcomingTaskTitle = nextTask?.Title ?? "No upcoming tasks";
                UpcomingTaskStartTime = nextTask?.StartTime ?? TimeSpan.Zero;
            }

            OnPropertyChanged(nameof(IsFocusActive));
            OnPropertyChanged(nameof(TaskHeader));
            OnPropertyChanged(nameof(TaskHeaderColor));
        }

        public void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
