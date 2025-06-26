using System;
using System.Collections.ObjectModel;

namespace FocusLock.Models
{
    public class UserModel
    {
        // User's display name
        public string UserName { get; set; } = "User";

        // Total time spent focusing
        public TimeSpan TotalFocusTime { get; set; }

        // Total time spent distracted
        public TimeSpan TotalDistractionTime { get; set; }

        // Number of tasks the user has completed
        public int TasksCompleted { get; set; }

        // Number of tasks the user has created
        public int TasksCreated { get; set; }

        // Collection of distractions being tracked for the user
        public ObservableCollection<Distraction> TrackedDistractions { get; set; } = new();
    }
}
