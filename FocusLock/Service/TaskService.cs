using FocusLock.Models;
using FocusLock.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

public static class TaskService
{
    // Events to notify UI or other parts when active or next tasks change or tasks update
    public static event Action<TaskItem?> ActiveTaskChanged;
    public static event Action<TaskItem?> NextTaskChanged;
    public static event Action TasksUpdated;

    private static TaskItem? _currentActiveTask;
    private static TaskItem? _currentNextTask;

    // Gets the currently active task, or null if none is active.
    public static TaskItem? ActiveTask => _currentActiveTask;

    // Gets the next upcoming task, or null if none.
    public static TaskItem? NextTask => _currentNextTask;


    // Loads all tasks and keeps them updated based on current time.
    public static async Task UpdateAsync()
    {
        var now = DateTime.Now.TimeOfDay;
        var tasks = await LoadTasksAsync();

        // Update completion status on all tasks
        foreach (var task in tasks)
        {
            task.CheckCompletionStatus();
        }

        // Determine the currently active task based on time and completion
        var active = tasks.FirstOrDefault(task => !task.IsCompleted && now >= task.StartTime && now < task.StartTime + task.Duration);
        if (_currentActiveTask?.ID != active?.ID)
        {
            _currentActiveTask = active;
            ActiveTaskChanged?.Invoke(_currentActiveTask);

            // Start or stop focus mode depending on active task presence
            if (_currentActiveTask != null)
                await FocusService.StartAsync();
            else
                FocusService.Stop();

            TasksUpdated?.Invoke();
        }

        // Determine the next upcoming task after the current time
        var next = tasks
            .Where(task => !task.IsCompleted && now < task.StartTime)
            .OrderBy(t => t.StartTime)
            .FirstOrDefault();

        if (_currentNextTask?.ID != next?.ID)
        {
            _currentNextTask = next;
            NextTaskChanged?.Invoke(_currentNextTask);
        }
    }

    // Returns the count of tasks marked as completed.
    public static async Task<int> GetCompletedCountAsync()
    {
        var tasks = await LoadTasksAsync();
        return tasks.Count(t => t.IsCompleted);
    }

    // Returns the total count of tasks created.
    public static async Task<int> GetCreatedCountAsync()
    {
        var tasks = await LoadTasksAsync();
        return tasks.Count;
    }

    #region Save/Load

    // File path for persisting tasks in app data folder
    private static readonly string SavePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FocusLock", "tasks.json");

    // JSON serialization options for pretty formatting
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    // Loads the task list from the JSON file asynchronously.
    // Returns an empty list if file is missing or on error.
    public static async Task<List<TaskItem>> LoadTasksAsync()
    {
        try
        {
            if (!File.Exists(SavePath))
                return new List<TaskItem>();

            string json = await File.ReadAllTextAsync(SavePath);
            return JsonSerializer.Deserialize<List<TaskItem>>(json, JsonOptions) ?? new List<TaskItem>();
        }
        catch
        {
            return new List<TaskItem>();
        }
    }

    // Saves the provided list of tasks to the JSON file asynchronously.
    // Logs errors but does not throw exceptions.
    public static async Task SaveTasksAsync(List<TaskItem> tasks)
    {
        try
        {
            var dir = Path.GetDirectoryName(SavePath);
            if (dir is not null)
                Directory.CreateDirectory(dir);

            string json = JsonSerializer.Serialize(tasks, JsonOptions);
            await File.WriteAllTextAsync(SavePath, json);
        }
        catch (Exception ex)
        {
            Logger.Log($"[TaskService] Failed to save tasks: {ex.Message}");
        }
    }

    #endregion 
}
