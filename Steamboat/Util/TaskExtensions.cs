using System;
using System.Threading.Tasks;

namespace Steamboat.Util
{
    internal static class TaskExtensions
    {
        public static async Task WithTimeout(this Task task, int timeoutMs)
        {
            if (await Task.WhenAny(task, Task.Delay(timeoutMs)) != task)
            {
                throw new Exception("Task timeout exceeded. ");
            }
        }
        
        public static async Task<T> WithTimeout<T>(this Task<T> task, int timeoutMs)
        {
            if (await Task.WhenAny(task, DelayTask<T>(timeoutMs)) == task)
            {
                return task.Result;
            }
            else
            {
                throw new Exception("Task timeout exceeded. ");
            }
        }

        private static async Task<T> DelayTask<T>(int timeout)
        {
            await Task.Delay(timeout);
            return default!;
        }
    }
}