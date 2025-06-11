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
        private static readonly DispatcherTimer timer = new()
        {
            Interval = TimeSpan.FromSeconds(0.1)
        };

        private static readonly List<Func<Task>> subscribers = new();

        static TickService()
        {
            timer.Tick += async (_, __) =>
            {
                foreach (var subscriber in subscribers.ToList())
                {
                    try
                    {
                        await subscriber();
                    }
                    catch (Exception ex)
                    {
                        
                    }
                }
            };

            timer.Start();
        }

        public static void Subscribe(Func<Task> callback)
        {
            if (!subscribers.Contains(callback)) subscribers.Add(callback);
        }

        public static void Unsubscribe(Func<Task> callback)
        {
            subscribers.Remove(callback);
        }
    }
}
