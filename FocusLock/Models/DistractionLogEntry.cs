using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FocusLock.Models
{
    public class DistractionLogEntry
    {
        public string ExePath { get; set; }
        public string Name { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }

        public TimeSpan Duration =>
            (EndTime ?? DateTime.Now) - StartTime;
    }

}
