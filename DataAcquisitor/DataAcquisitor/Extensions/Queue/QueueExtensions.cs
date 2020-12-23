using System.Collections.Generic;

namespace DataAcquisitor.Extensions.Queue
{
    public static class QueueExtensions
    {
        public static void EnqueueRange<T>(this Queue<T> queue, T[] enu)
        {
            foreach (T obj in enu)
                queue.Enqueue(obj);
        }
    }
}
