using FocusLock.Helper;
using FocusLock.Models;
using FocusLock.Service;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FocusLock.ViewModels
{
    public class DashboardViewModel : BaseViewModel, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // Singleton instance for static tick access
        private static DashboardViewModel? _instance;

        // Static method to trigger periodic update
        public static void Tick() => _instance?.LoadDashboardDataAsync();

        // Collection of distractions currently tracked, bound to UI
        public ObservableCollection<Distraction> TrackedDistractions { get; } = new();

        // Top distractions sorted by tracked time, bound to UI
        public ObservableCollection<Distraction> TopDistractions { get; } = new();

        // User model holding aggregated user statistics
        public UserModel User { get; } = new();

        // Constructor initializes singleton instance and starts data loading
        public DashboardViewModel()
        {
            _instance = this;
            _ = LoadDashboardDataAsync();
        }

        // Loads all dashboard data: distractions, user stats, task info
        private async Task LoadDashboardDataAsync()
        {
            await RefreshTrackedDistractions();
            await RefreshUserStats();
            SyncFromTaskService();
        }

        // Loads and sorts tracked distractions by total tracked time
        // Updates ObservableCollections on UI thread
        private async Task RefreshTrackedDistractions()
        {
            var allDistractions = await DistractionService.LoadDistractionsAsync();
            var sorted = allDistractions
                .Where(d => d.TotalTrackedTime.TotalSeconds > 0)
                .OrderByDescending(d => d.TotalTrackedTime)
                .ToList();

            var top3 = sorted.Take(3).ToList();

            App.Current.Dispatcher.Invoke(() =>
            {
                TrackedDistractions.Clear();
                foreach (var distraction in sorted.Take(5))
                    TrackedDistractions.Add(distraction);

                TopDistractions.Clear();
                foreach (var d in top3)
                    TopDistractions.Add(d);
            });
        }

        // Refreshes aggregated user statistics asynchronously
        private async Task RefreshUserStats()
        {
            var distractions = await DistractionService.LoadDistractionsAsync();
            User.TotalDistractionTime = distractions.Aggregate(TimeSpan.Zero, (acc, d) => acc + d.TotalTrackedTime);
            User.TasksCompleted = await TaskService.GetCompletedCountAsync();
            User.TasksCreated = await TaskService.GetCreatedCountAsync();
            User.TotalFocusTime = await FocusService.GetTotalFocusTimeAsync();
            Logger.Log($"User stats updated: {User.TotalDistractionTime}, {User.TasksCompleted}, {User.TasksCreated}, {User.TotalFocusTime}");
            User.UserName = Environment.UserName;
        }

        #region Task Info

        private string _upcomingTaskTitle = "Loading...";
        public string UpcomingTaskTitle
        {
            get => _upcomingTaskTitle;
            set { _upcomingTaskTitle = value; OnPropertyChanged(); }
        }

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
                }
            }
        }

        // Header string that changes depending on task running status
        public string TaskHeader => IsTaskRunning ? "Active Task" : "Upcoming Task";

        // Synchronizes task-related properties from TaskService
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

            OnPropertyChanged(nameof(TaskHeader));
        }

        // Raises PropertyChanged event for data binding
        public void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        #endregion
    }
}
