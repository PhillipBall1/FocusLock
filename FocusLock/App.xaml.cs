using FocusLock.Helper;
using FocusLock.Models;
using FocusLock.Service;
using FocusLock.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace FocusLock
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            TickService.Subscribe(TaskService.UpdateAsync);
            //TickService.Subscribe(DistractionService.TrackDistractionUsageAsync);

            await DistractionLogService.LoadFromFile();
            await DistractionService.InitializeAsync();
        }
    }

}
