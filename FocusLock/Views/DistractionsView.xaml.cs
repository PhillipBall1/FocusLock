using FocusLock.Models;
using FocusLock.Service;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FocusLock.Views
{
    public partial class DistractionsView : UserControl
    {
        // List holding all distractions shown in the UI
        private List<Distraction> distractions = new();

        public DistractionsView()
        {
            InitializeComponent();
            LoadDistractionsAsync(); // Load distractions on startup
        }

        // Load active and stored distractions, merge them, and update the UI list
        private async void LoadDistractionsAsync()
        {
            var stored = await DistractionService.LoadDistractionsAsync(); // Previously saved distractions
            var active = DistractionService.GetActiveProcesses(); // Current running distractions

            // Merge stored distractions with newly active ones that are not in stored list yet
            var merged = stored.Concat(
                active.Where(activeDis =>
                    !stored.Any(storedDis =>
                        // Match by executable path or root process name to avoid duplicates
                        (!string.IsNullOrEmpty(storedDis.RootExePath) && storedDis.RootExePath == activeDis.RootExePath) ||
                        storedDis.RootProcessName == activeDis.RootProcessName
                    )
                )).ToList();

            // Keep the IsDistraction flag from stored items for matching distractions
            foreach (var distraction in merged)
            {
                var matched = stored.FirstOrDefault(storedDis =>
                    (!string.IsNullOrEmpty(storedDis.RootExePath) && storedDis.RootExePath == distraction.RootExePath) ||
                    storedDis.RootProcessName == distraction.RootProcessName);

                if (matched != null)
                    distraction.IsDistraction = matched.IsDistraction;
            }

            // Sort distractions: non-distractions first, then alphabetically by root process name
            distractions = merged
                .OrderBy(dis => dis.IsDistraction)
                .ThenBy(dis => dis.RootProcessName)
                .ToList();

            // Bind the sorted distractions list to the UI control
            DistractionItemsControl.ItemsSource = distractions;
        }

        // Handle clicking on a distraction card: toggle IsDistraction and save changes
        private async void Card_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is Distraction distraction)
            {
                distraction.IsDistraction = !distraction.IsDistraction;
                await SaveDistractionsAsync();
            }
        }

        // Save the current distractions list to persistent storage
        private async Task SaveDistractionsAsync()
        {
            var selected = distractions.ToList();
            await DistractionService.SaveDistractionsAsync(selected);
        }
    }
}
