using FocusLock.Models;
using FocusLock.Service;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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

        // OxyPlot model for distraction chart
        private PlotModel _distractionChart;
        public PlotModel DistractionChart
        {
            get => _distractionChart;
            private set { _distractionChart = value; OnPropertyChanged(nameof(DistractionChart)); }
        }

        // Collection of distractions currently tracked, bound to UI
        public ObservableCollection<Distraction> TrackedDistractions { get; } = new();

        // Top distractions sorted by tracked time, bound to UI
        public ObservableCollection<Distraction> TopDistractions { get; } = new();

        // User model holding user statistics
        public UserModel User { get; } = new();

        // Constructor initializes singleton instance and starts data loading
        public DashboardViewModel()
        {
            _instance = this;
            _ = LoadDashboardDataAsync();
            TrackedDistractions.CollectionChanged += (_, __) => BuildDistractionChart();
            BuildDistractionChart();
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
                foreach (var distraction in sorted.Take(7)) TrackedDistractions.Add(distraction);

                TopDistractions.Clear();
                foreach (var d in top3) TopDistractions.Add(d);
            });
        }

        // Refreshes aggregated user statistics asynchronously
        private async Task RefreshUserStats()
        {
            var distractions = await DistractionService.LoadDistractionsAsync();
            User.TotalDistractionTime = distractions.Aggregate(TimeSpan.Zero, (acc, d) => acc + d.TotalTrackedTime);
            User.TasksCompleted = TaskService.GetCompletedCount();
            User.TasksCreated = TaskService.GetCreatedCount();
            User.TotalFocusTime = await FocusService.GetTotalFocusTimeAsync();
            Logger.Log($"User stats updated: {User.TotalDistractionTime}, {User.TasksCompleted}, {User.TasksCreated}, {User.TotalFocusTime}");
            User.UserName = Environment.UserName;
        }

        // Builds the distraction chart using OxyPlot
        private void BuildDistractionChart()
        {
            var model = new PlotModel();
            OxyColor background = Properties.Settings.Default.IsDarkMode ? OxyColor.Parse("#242424") : OxyColor.Parse("#FFFFFF");
            OxyColor textBorder = Properties.Settings.Default.IsDarkMode ? OxyColor.Parse("#F8F8F8") : OxyColor.Parse("#556070");
            model.Background = background;
            model.DefaultFontSize = 15;
            model.SubtitleFontWeight = FontWeights.Bold;
            model.PlotAreaBorderColor = textBorder;
            model.PlotAreaBorderThickness = new OxyThickness(3, 3, 3, 3);
            model.TextColor = textBorder;

            model.ZoomAllAxes(0);

            var categoryAxis = new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                TicklineColor = textBorder,
                Key = "cats",
                Angle = 25,
                IsZoomEnabled = false,
                FontWeight = FontWeights.Bold,
            };

            var valueAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Minutes",
                Key = "values",
                TicklineColor = textBorder,
                MinimumPadding = 0,
                AbsoluteMinimum = 0,
                IsZoomEnabled = false,
                FontWeight = FontWeights.Bold,
            };
            valueAxis.MaximumPadding = 0.1; 

            categoryAxis.StartPosition = 1;
            categoryAxis.EndPosition = 0;


            foreach (var d in TrackedDistractions) categoryAxis.Labels.Add(d.DisplayName);

            model.Axes.Add(categoryAxis);
            model.Axes.Add(valueAxis);

            var series = new BarSeries
            {
                XAxisKey = valueAxis.Key,   
                YAxisKey = categoryAxis.Key, 
                LabelPlacement = LabelPlacement.Outside,
                LabelFormatString = "{0:0}",
                FontWeight = FontWeights.Bold,
            };

            foreach (var d in TrackedDistractions)
            {
                var barItem = new BarItem
                {
                    Value = d.TotalTrackedTime.TotalMinutes,
                    Color = textBorder
                };
                series.Items.Add(barItem);
            }

            model.Series.Add(series);

            DistractionChart = model;
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
