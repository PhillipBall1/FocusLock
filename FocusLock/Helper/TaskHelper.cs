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
        public static async Task<List<TaskItem>> GetAllTasksAsync()
        {
            return await TaskService.LoadTasksAsync();
        }

        public static async Task<TaskItem> GetActiveTaskAsync()
        {
            var now = DateTime.Now.TimeOfDay;
            var tasks = await GetAllTasksAsync();

            return tasks.FirstOrDefault(task =>
                task.StartTime <= now &&
                now < task.StartTime + task.Duration &&
                !task.IsCompleted);
        }

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
