using FocusLock.Helper;
using FocusLock.Models;
using FocusLock.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.PropertyGridInternal;

public static class TaskService
{
    // Events to notify UI or other parts when active or next tasks change or tasks update
    public static event Action<TaskItem?> ActiveTaskChanged;
    public static event Action<TaskItem?> NextTaskChanged;
    public static event Action TasksUpdated;

    private static TaskItem? _currentActiveTask;
    private static TaskItem? _currentNextTask;

    public static TaskItem? ActiveTask => _currentActiveTask;
    public static TaskItem? NextTask => _currentNextTask;

    // Persistent task list that the UI binds to
    public static ObservableCollection<TaskItem> CurrentTasks { get; } = new();

    private static readonly string SavePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "FocusLock", "tasks.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    // Update all task states and perform background actions
    public static async Task UpdateAsync()
    {
        var now = DateTime.Now.TimeOfDay;
        var tasks = CurrentTasks.ToList();
        var toRemove = new List<TaskItem>();
        bool modified = false;

        foreach (var task in tasks)
        {
            var wasCompleted = task.IsCompleted;

            // Check if completed
            if (now >= task.StartTime + task.Duration && task.HasNotified)
            {
                task.IsCompleted = true;
                task.HasNotified = false;
                await SaveTasksAsync();
            }

            // Remove if completed and not recurring
            if (task.IsCompleted && !task.IsRecurring)
            {
                toRemove.Add(task);
                modified = true;
            }

            // Reset recurring tasks for the next day
            if (task.IsCompleted && task.IsRecurring)
            {
                if(now < task.StartTime)
                {
                    task.IsCompleted = false;
                    await SaveTasksAsync();
                }
            }

            if (!FocusLock.Properties.Settings.Default.ShowNotifications) continue;

            // Notify user if 20 minutes before task
            TimeSpan notifyTime = task.StartTime - TimeSpan.FromMinutes(20);
            if (now >= notifyTime && now < task.StartTime && !task.HasNotified)
            {
                task.HasNotified = true;
                modified = true;
                NotificationHelper.ShowNotification(
                    "Upcoming Task",
                    $"{task.Title} starts in {(int)(task.StartTime - now).TotalMinutes} minutes."
                );
            }
        }

        // Remove completed tasks
        foreach (var task in toRemove) CurrentTasks.Remove(task);

        // Get the active task, if exists, start focus
        var active = tasks.FirstOrDefault(t => !t.IsCompleted && now >= t.StartTime && now < t.StartTime + t.Duration);
        if (_currentActiveTask?.ID != active?.ID)
        {
            _currentActiveTask = active;
            ActiveTaskChanged?.Invoke(_currentActiveTask);

            if (_currentActiveTask != null)
                await FocusService.StartAsync();
            else
                FocusService.Stop();

            TasksUpdated?.Invoke();
        }

        // Get then next task
        var next = tasks
            .Where(t => !t.IsCompleted && now < t.StartTime)
            .OrderBy(t => t.StartTime)
            .FirstOrDefault();

        if (_currentNextTask?.ID != next?.ID)
        {
            _currentNextTask = next;
            NextTaskChanged?.Invoke(_currentNextTask);
        }

        if (modified || toRemove.Count > 0)
            await SaveTasksAsync();
    }

    public static int GetCompletedCount() => CurrentTasks.Count(t => t.IsCompleted);
    public static int GetCreatedCount() => CurrentTasks.Count;

    #region Save / Load

    public static async Task LoadTasksAsync()
    {
        try
        {
            var dir = Path.GetDirectoryName(SavePath);
            if (dir is not null)
                Directory.CreateDirectory(dir);

            if (!File.Exists(SavePath))
            {
                CurrentTasks.Clear();
                return;
            }

            string json = await File.ReadAllTextAsync(SavePath);
            var loaded = JsonSerializer.Deserialize<List<TaskItem>>(json, JsonOptions) ?? new List<TaskItem>();

            Application.Current.Dispatcher.Invoke(() =>
            {
                CurrentTasks.Clear();
                foreach (var task in loaded)
                    CurrentTasks.Add(task);
            });
        }
        catch (Exception ex)
        {
            Logger.Log($"[TaskService] Failed to load tasks: {ex.Message}");
            CurrentTasks.Clear();
        }
    }

    public static async Task SaveTasksAsync()
    {
        try
        {
            var dir = Path.GetDirectoryName(SavePath);
            if (dir is not null)
                Directory.CreateDirectory(dir);
            var snapshot = CurrentTasks.ToList();
            string json = JsonSerializer.Serialize(snapshot, JsonOptions);

            await File.WriteAllTextAsync(SavePath, json);
        }
        catch (Exception ex)
        {
            Logger.Log($"[TaskService] Failed to save tasks: {ex.Message}");
        }
    }

    #endregion
}
