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
        private List<Distraction> distractions = new();

        public DistractionsView()
        {
            InitializeComponent();
            LoadDistractionsAsync();
        }

        private async void LoadDistractionsAsync()
        {
            var active = DistractionService.GetActiveProcesses();
            var stored = await DistractionService.LoadDistractionsAsync();

            var merged = stored.Concat(
                active.Where(activeDis =>
                    !stored.Any(storedDis =>
                        !string.IsNullOrEmpty(storedDis.RootExePath) &&
                        storedDis.RootExePath == activeDis.RootExePath
                        || storedDis.RootProcessName == activeDis.RootProcessName
                    )
                )).ToList();

            foreach (var distraction in merged)
            {
                var matched = stored.FirstOrDefault(storedDis =>
                    (!string.IsNullOrEmpty(storedDis.RootExePath) && storedDis.RootExePath == distraction.RootExePath) ||
                    storedDis.RootProcessName == distraction.RootProcessName);

                if (matched != null)
                    distraction.IsDistraction = matched.IsDistraction;
            }

            distractions = merged
                .OrderBy(dis => dis.IsDistraction)
                .ThenBy(dis => dis.RootProcessName)
                .ToList();

            DistractionItemsControl.ItemsSource = distractions;
        }

        private async void Card_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is Distraction distraction)
            {
                distraction.IsDistraction = !distraction.IsDistraction;
                await SaveDistractionsAsync();
            }
        }

        private async Task SaveDistractionsAsync()
        {
            var selected = distractions.Where(d => d.IsDistraction).ToList();
            await DistractionService.SaveDistractionsAsync(selected);
        }
    }
}
