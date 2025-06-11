using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FocusLock.Models
{
    public class TaskItem
    {
        public Guid ID { get; set; } = Guid.NewGuid();
        public string? Title { get; set; }
        public TimeSpan StartTime { get; set; }
        public bool IsRecurring { get; set; } = false;
        public TimeSpan Duration { get; set; }
        public bool IsCompleted { get; set; } = false;

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
