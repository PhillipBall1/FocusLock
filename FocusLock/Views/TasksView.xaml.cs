using FocusLock.Models;
using FocusLock.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace FocusLock.Views
{
    public partial class TasksView : UserControl
    { 
        public TasksView()
        {
            InitializeComponent();
            DataContext = new TasksViewModel();            
        }

        private List<TaskItem> taskItems = new();

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            taskItems = await TaskService.LoadTasksAsync();
            TasksListView.ItemsSource = taskItems;
        }

        private async void AddTask_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
                return;

            bool normalTime = StartTimeTextBox.Text.ToLower().Contains("am") || StartTimeTextBox.Text.ToLower().Contains("pm");

            TimeSpan startTime;

            if (!TimeSpan.TryParse(StartTimeTextBox.Text, out startTime) && !normalTime)
            {
                MessageBox.Show("Please enter a valid time like 2:00 PM or 14:00");
                return;
            }

            if (normalTime)
            {
                DateTime parsedDateTime;
                if (DateTime.TryParseExact(StartTimeTextBox.Text, "h:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDateTime))
                {
                    startTime = parsedDateTime.TimeOfDay;
                }
                else
                {
                    MessageBox.Show("Please enter a valid time like 2:00 PM or 14:00");
                    return;
                }
            }

            TimeSpan duration;

            string input = DurationTextBox.Text.Trim();

            if (input.Contains(":"))
            {
                MessageBox.Show("Please enter a number of minutes.");
                return;
            }
            else
            {
                if (int.TryParse(input, out int minutes))
                {
                    duration = TimeSpan.FromMinutes(minutes);
                }
                else
                {
                    MessageBox.Show("Please enter a valid number of minutes.");
                    return;
                }
            }

            var newTask = new TaskItem
            {
                Title = TitleTextBox.Text,
                StartTime = startTime,
                Duration = duration,
                IsRecurring = RecurringCheckBox.IsChecked == true,
                IsCompleted = false
            };

            taskItems.Add(newTask);
            TasksListView.Items.Refresh();
            await TaskService.SaveTasksAsync(taskItems);

            TitleTextBox.Text = "";
            DurationTextBox.Text = "";
            StartTimeTextBox.Text = "";
            RecurringCheckBox.IsChecked = false;
        }


        private async void RemoveTask_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to delete this task?", "Confirm Delete", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes) return;

            if (sender is Button button && button.Tag is TaskItem task)
            {
                taskItems.Remove(task);
                TasksListView.Items.Refresh();
                await TaskService.SaveTasksAsync(taskItems);
            }
        }
    }
}

