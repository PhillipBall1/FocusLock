using FocusLock.Helper;
using FocusLock.Models;
using FocusLock.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Threading;

public static class TaskService
{
    public static event Action<TaskItem?> ActiveTaskChanged;
    public static event Action<TaskItem?> NextTaskChanged;
    public static event Action TasksUpdated;

    private static TaskItem? _currentActiveTask;
    private static TaskItem? _currentNextTask;

    public static TaskItem? ActiveTask => _currentActiveTask;
    public static TaskItem? NextTask => _currentNextTask;

    public static async Task UpdateAsync()
    {
        var now = DateTime.Now.TimeOfDay;
        var tasks = await LoadTasksAsync();

        foreach (var task in tasks)
        {
            task.CheckCompletionStatus();
        }

        var active = tasks.FirstOrDefault(task => !task.IsCompleted && now >= task.StartTime && now < task.StartTime + task.Duration);
        if (_currentActiveTask?.ID != active?.ID)
        {
            _currentActiveTask = active;
            ActiveTaskChanged?.Invoke(_currentActiveTask);

            if (_currentActiveTask != null) await FocusService.StartAsync();
            else FocusService.Stop();

            TasksUpdated?.Invoke();
        }

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

    #region Save/Load

    private static readonly string SavePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "FocusLock",
        "tasks.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

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

    public static async Task SaveTasksAsync(List<TaskItem> tasks)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SavePath)!);
            string json = JsonSerializer.Serialize(tasks, JsonOptions);
            await File.WriteAllTextAsync(SavePath, json);
        }
        catch (Exception ex)
        {

        }
    }

    #endregion 
}
