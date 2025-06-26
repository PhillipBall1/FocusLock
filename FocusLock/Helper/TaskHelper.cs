using FocusLock.Models;
using FocusLock.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FocusLock.Helper
{
    public static class TaskHelper
    {
        // Loads all tasks asynchronously from the TaskService
        public static async Task<List<TaskItem>> GetAllTasksAsync()
        {
            return await TaskService.LoadTasksAsync();
        }

        // Returns the task that is currently active (based on current time and completion status)
        public static async Task<TaskItem> GetActiveTaskAsync()
        {
            var now = DateTime.Now.TimeOfDay;
            var tasks = await GetAllTasksAsync();

            return tasks.FirstOrDefault(task =>
                task.StartTime <= now &&
                now < task.StartTime + task.Duration &&
                !task.IsCompleted);
        }

        // Returns the next upcoming task that has not yet started and is not completed
        public static async Task<TaskItem> GetNextTaskAsync()
        {
            var now = DateTime.Now.TimeOfDay;
            var tasks = await GetAllTasksAsync();

            return tasks
                .Where(task => task.StartTime > now && !task.IsCompleted)
                .OrderBy(task => task.StartTime)
                .FirstOrDefault();
        }
    }
}
