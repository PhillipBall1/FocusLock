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
        // Holds the list of tasks loaded and managed in this view
        private List<TaskItem> taskItems = new();

        public TasksView()
        {
            InitializeComponent();
            DataContext = new TasksViewModel();
        }

        // Load tasks when the view is loaded
        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            taskItems = await TaskService.LoadTasksAsync();
            TasksListView.ItemsSource = taskItems;
        }

        // Called when user clicks the Add Task button
        private async void AddTask_Click(object sender, RoutedEventArgs e)
        {
            // Validate the task title
            if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
                return;

            // Check if time contains AM/PM to handle parsing
            bool normalTime = StartTimeTextBox.Text.ToLower().Contains("am") || StartTimeTextBox.Text.ToLower().Contains("pm");

            TimeSpan startTime;

            // Try parse as TimeSpan if no AM/PM present
            if (!TimeSpan.TryParse(StartTimeTextBox.Text, out startTime) && !normalTime)
            {
                MessageBox.Show("Please enter a valid time like 2:00 PM or 14:00");
                return;
            }

            // If AM/PM time, parse as DateTime and extract TimeOfDay
            if (normalTime)
            {
                if (DateTime.TryParseExact(StartTimeTextBox.Text, "h:mm tt", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDateTime))
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

            // Reject input with colon to enforce minute input only
            if (input.Contains(":"))
            {
                MessageBox.Show("Please enter a number of minutes.");
                return;
            }
            else
            {
                // Parse minutes input as integer
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

            // Create new task item with input values
            var newTask = new TaskItem
            {
                Title = TitleTextBox.Text,
                StartTime = startTime,
                Duration = duration,
                IsRecurring = RecurringCheckBox.IsChecked == true,
                IsCompleted = false
            };

            // Add the new task to list, refresh UI, and save
            taskItems.Add(newTask);
            TasksListView.Items.Refresh();
            await TaskService.SaveTasksAsync(taskItems);

            // Clear input fields
            TitleTextBox.Text = "";
            DurationTextBox.Text = "";
            StartTimeTextBox.Text = "";
            RecurringCheckBox.IsChecked = false;
        }

        // Called when user clicks the Remove Task button on a task item
        private async void RemoveTask_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to delete this task?", "Confirm Delete", MessageBoxButton.YesNo);
            if (result != MessageBoxResult.Yes) return;

            // Remove selected task from list, refresh UI, and save
            if (sender is Button button && button.Tag is TaskItem task)
            {
                taskItems.Remove(task);
                TasksListView.Items.Refresh();
                await TaskService.SaveTasksAsync(taskItems);
            }
        }
    }
}
