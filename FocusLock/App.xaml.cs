using FocusLock.Service;
using FocusLock.ViewModels;
using System.Threading.Tasks;
using System.Windows;

namespace FocusLock
{
    public partial class App : Application
    {
        // Called when application starts
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            InitializeAsync();
        }

        // Asynchronously initialize services and subscriptions
        private async void InitializeAsync()
        {
            // Subscribe main update and tracking tasks to TickService timer
            TickService.Subscribe(TaskService.UpdateAsync);
            TickService.Subscribe(DistractionService.TrackDistractionUsageAsync);
            TickService.Subscribe(() =>
            {
                DashboardViewModel.Tick();
                return Task.CompletedTask;
            });

            // Load total focus time from storage
            await FocusService.LoadTotalFocusTimeAsync();

            // Load distractions data from storage
            await DistractionService.InitializeAsync();
        }
    }
}
