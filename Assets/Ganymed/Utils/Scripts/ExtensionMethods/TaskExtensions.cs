using System;
using System.Threading;
using System.Threading.Tasks;

namespace Ganymed.Utils.ExtensionMethods
{
    public static class TaskExtensions
    {
        public static Task Then(this Task task, Action<Task> func)
        {
            return task.ContinueWith(func, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public static Task Then<X>(this Task<X> task, Action<Task<X>> func)
        {
            return task.ContinueWith(func, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public static Task<T> Then<T>(this Task task, Func<Task, T> func)
        {
            return task.ContinueWith<T>(func, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public static Task<T> Then<X, T>(this Task<X> task, Func<Task<X>, T> func)
        {
            return task.ContinueWith<T>(func, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}
