using FocusLock.Models;
using FocusLock.Service;
using FocusLock.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace FocusLock.Views
{
    public partial class FocusView : UserControl
    {
        private FocusViewModel viewModel;

        public FocusView()
        {
            InitializeComponent();
            viewModel = new FocusViewModel();
            DataContext = viewModel;
        }

        private async void FocusButton_Click(object sender, RoutedEventArgs e)
        {
            if (viewModel.IsTaskRunning) return;
             
            if (!viewModel.IsFocusActive)
                await FocusService.StartAsync();
            else
                FocusService.Stop();

            viewModel.OnPropertyChanged(nameof(viewModel.IsFocusActive));
        }
    }
}
