using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using Server.Commands;

namespace Server
{
    public static class TimerRegistry
    {
        public static readonly bool Debug = false;

        public static void Initialize()
        {
            if (Debug)
            {
                CommandSystem.Register("CheckTimers", AccessLevel.Administrator, e =>
                {
                    foreach (var kvp in Timers)
                    {
                        Utility.WriteConsoleColor(ConsoleColor.Green, "Timer ID: {0}", kvp.Key);

                        Console.WriteLine("Delay/Interval: {0}", kvp.Value.Interval);
                        Console.WriteLine("Timer Priority: {0}", kvp.Value.Priority);
                    }
                });
            }
        }

        public static Dictionary<string, Timer> Timers { get; set; } = new Dictionary<string, Timer>();

        public static void Register<T>(string id, T instance, TimeSpan duration, Action<T> callback)
        {
            Register(id, instance, duration, TimeSpan.Zero, true, true, callback);
        }

        public static void Register<T>(string id, T instance, TimeSpan duration, TimeSpan delay, Action<T> callback)
        {
            Register(id, instance, duration, delay, true, true, callback);
        }

        public static void Register<T>(string id, T instance, TimeSpan duration, bool removeOnExpire, Action<T> callback)
        {
            Register(id, instance, duration, TimeSpan.Zero, removeOnExpire, true, callback);
        }

        public static void Register<T>(string id, T instance, TimeSpan duration, TimeSpan delay, bool removeOnExpire, Action<T> callback)
        {
            Register(id, instance, duration, delay, removeOnExpire, true, callback);
        }

        public static void Register<T>(string id, T instance, TimeSpan duration, TimeSpan delay, bool removeOnExpire, bool checkDeleted, Action<T> callback)
        {
            var timer = GetTimer(id, instance);

            if(Debug) Console.WriteLine("Registering: {0} - {1}...", id, instance);

            if (timer == null)
            {
                if (Debug) Console.WriteLine("Timer not Found, creating new one...");
                timer = new RegistryTimer<T>(delay == TimeSpan.Zero ? ProcessDelay(duration) : delay, callback, removeOnExpire, checkDeleted);
                Timers[id] = timer;
                timer.Start();
            }
            else if(Debug)
            {
                Console.WriteLine("Timer Found, adding to existing registry...");
            }

            if (!timer.Registry.ContainsKey(instance))
            {
                if (Debug) Console.WriteLine("Adding {0} to the timer registry!", instance);
                timer.Registry[instance] = DateTime.UtcNow + duration;
            }
            else if (Debug)
            {
                Console.WriteLine("Instnace already exists in the timer registry!");
            }
        }

        public static void RemoveFromRegistry<T>(string id, T instance)
        {
            var timer = GetTimer(id, instance);

            if (timer != null)
            {
                if (timer.Registry.ContainsKey(instance))
                {
                    timer.Registry.Remove(instance);
                    if (Debug) Console.WriteLine("Removing {0} from the registry", instance);

                    if (timer.Registry.Count == 0)
                    {
                        UnregisterTimer(timer);
                    }
                }
            }
        }

        public static bool UpdateRegistry<T>(string id, T instance, TimeSpan duration)
        {
            var timer = GetTimerFor(id, instance);

            if (Debug) Console.WriteLine("Updating Registry for {0} - {1}...", id, instance);
            if (timer != null)
            {
                if (Debug) Console.WriteLine("Complete!");
                timer.Registry[instance] = DateTime.UtcNow + duration;
                return true;
            }
            else
            {
                Console.WriteLine("Failed, timer not found");
            }

            return false;
        }

        public static RegistryTimer<T> GetTimer<T>(string id, T instance)
        {
            if (Timers.ContainsKey(id))
            {
                if (!(Timers[id] is RegistryTimer<T>))
                {
                    throw new ArgumentException("Type names must be the same for each identifier. T instance is not the same type as RegistryTimer<T>", instance.GetType().Name);
                }

                return Timers[id] as RegistryTimer<T>;
            }

            return null;
        }

        public static RegistryTimer<T> GetTimerFor<T>(string id, T instance)
        {
            if (Timers.ContainsKey(id))
            {
                if (Timers[id] is RegistryTimer<T> timer && timer.Registry.ContainsKey(instance))
                {
                    return Timers[id] as RegistryTimer<T>;
                }
            }

            return null;
        }

        public static bool HasTimer<T>(string id, T instance)
        {
            return GetTimerFor(id, instance) != null;
        }

        public static void UnregisterTimer(Timer timer)
        {
            timer.Stop();
            var id = GetTimerID(timer);

            if (!string.IsNullOrEmpty(id) && Timers.ContainsKey(id))
            {
                if (Debug) Console.WriteLine("Remove {0} from the timer list", id);
                Timers.Remove(id);
            }
        }

        public static string GetTimerID(Timer timer)
        {
            foreach (var kvp in Timers)
            {
                if (kvp.Value == timer)
                {
                    return kvp.Key;
                }
            }

            return string.Empty;
        }

        public static TimeSpan ProcessDelay(TimeSpan duration)
        {
            var seconds = duration.TotalSeconds;

            if (seconds >= 86400) // 1 day
            {
                return TimeSpan.FromMinutes(5);
            }
            else if (seconds >= 3600) // 1 hour
            {
                return TimeSpan.FromMinutes(1);
            }
            else if (seconds >= 600) // 10 minutes
            {
                return TimeSpan.FromSeconds(1);
            }

            return TimeSpan.FromMilliseconds(500);
        }
    }

    public class RegistryTimer<T> : Timer
    {
        public Dictionary<T, DateTime> Registry { get; set; } = new Dictionary<T, DateTime>();

        public Action<T> Callback { get; set; }
        public bool RemoveOnExpire { get; set; }
        public bool CheckDeleted { get; set; }

        public RegistryTimer(TimeSpan delay, Action<T> callback, bool removeOnExpire, bool checkDeleted)
            : base(delay, delay)
        {
            Callback = callback;
            RemoveOnExpire = removeOnExpire;
            CheckDeleted = checkDeleted;
        }

        protected override void OnTick()
        {
            List<T> instances = Registry.Keys.Where(v => Registry[v] < DateTime.UtcNow || (CheckDeleted && v is IEntity e && e.Deleted)).ToList();

            if (instances.Count > 500)
            {
                Parallel.ForEach(instances, instance =>
                {
                    if (Registry.ContainsKey(instance))
                    {
                        if (IsDeleted(instance))
                        {
                            if (TimerRegistry.Debug) Console.WriteLine("Removing from Registry [Deleted]: {0}", instance);
                            Registry.Remove(instance);
                        }
                        else
                        {
                            if (Callback != null)
                            {
                                Callback(instance);
                            }

                            if (IsExpired(instance))
                            {
                                if (TimerRegistry.Debug) Console.WriteLine("Removing from Registry [Processed]: {0}", instance);
                                Registry.Remove(instance);
                            }
                        }
                    }
                });
            }
            else
            {
                for (int i = 0; i < instances.Count; i++)
                {
                    var instance = instances[i];

                    if (IsDeleted(instance))
                    {
                        if (TimerRegistry.Debug) Console.WriteLine("Removing from Registry [Deleted]: {0}", instance);
                        Registry.Remove(instance);
                    }
                    else
                    {
                        if (Callback != null)
                        {
                            //if (TimerRegistry.Debug) Console.WriteLine("Callback");
                            Callback(instance);
                        }

                        if (IsExpired(instance))
                        {
                            if (TimerRegistry.Debug) Console.WriteLine("Removing from Registry [Processed]: {0}", instance);
                            Registry.Remove(instance);
                        }
                    }
                }

                ColUtility.Free(instances);

                if (Registry.Count == 0)
                {
                    TimerRegistry.UnregisterTimer(this);
                }
            }
        }

        private bool IsExpired(T instance)
        {
            return RemoveOnExpire && (!Registry.ContainsKey(instance) || Registry[instance] < DateTime.UtcNow);
        }

        private bool IsDeleted(T instance)
        {
            return CheckDeleted && instance is IEntity e && e.Deleted;
        }
    }
}
