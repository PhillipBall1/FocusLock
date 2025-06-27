using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace FocusLock.Service
{

    public static class TickService
    {
        // DispatcherTimer runs on the UI thread and raises Tick events
        private static readonly DispatcherTimer timer = new()
        {
            Interval = TimeSpan.FromSeconds(0.1) // Tick every 100 ms
        };

        // List of async subscriber callbacks to invoke on each tick
        private static readonly List<Func<Task>> subscribers = new();

        // Static constructor sets up timer and starts it immediately
        static TickService()
        {
            timer.Tick += async (_, __) =>
            {
                foreach (var subscriber in subscribers.ToList())
                {
                    try
                    {
                        // Await each subscriber callback asynchronously
                        await subscriber();
                    }
                    catch (Exception ex)
                    {

                    }
                }
            };

            timer.Start();
        }


        // Registers a subscriber callback to be called on each tick
        public static void Subscribe(Func<Task> callback)
        {
            if (!subscribers.Contains(callback))
                subscribers.Add(callback);
        }

        // Unregisters a previously registered subscriber callback.
        
        public static void Unsubscribe(Func<Task> callback)
        {
            subscribers.Remove(callback);
        }
    }
}
