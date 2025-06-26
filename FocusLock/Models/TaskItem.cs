using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FocusLock.Models
{
    /*
     * This class represents a single task in the FocusLock app.
     * It contains properties for the task's ID, title, start time, duration, recurrence, and completion status.
     * It also has a method to update the completion status based on the current time.
     */

    public class TaskItem
    {
        // Unique identifier for the task
        public Guid ID { get; set; } = Guid.NewGuid();

        // Title or name of the task
        public string? Title { get; set; }

        // Start time of the task (time of day)
        public TimeSpan StartTime { get; set; }

        // Whether the task repeats regularly
        public bool IsRecurring { get; set; } = false;

        // Duration of the task
        public TimeSpan Duration { get; set; }

        // Whether the task has been completed
        public bool IsCompleted { get; set; } = false;

        // Updates IsCompleted if the current time is past the task's end time
        public void CheckCompletionStatus()
        {
            var now = DateTime.Now.TimeOfDay;
            if (now >= StartTime + Duration)
            {
                IsCompleted = true;
            }
        }
    }
}
