using System;
using System.Collections.Generic;
using System.Threading;

namespace Hw3.Tests;

public static class Concurrency
{
    private static int _index;

    public static int Index => _index;

    private static readonly ManualResetEvent Event = new(false);

    private static readonly object Locker = new();
    
    public static int IncrementWithLock(int threadCount, int iterations)
    {
        return DoIncrement(threadCount, iterations, () =>
        {
            lock (Locker)
            {
                _index++;
            }
        });
    }
    
    public static int IncrementWithInterlocked(int threadCount, int iterations)
    {
        return DoIncrement(threadCount, iterations, () =>
        {
            Interlocked.Increment(ref _index);
        });
    }
    
    public static int Increment(int threadCount, int iterations)
    {
        return DoIncrement(threadCount, iterations, () => _index++);
    }

    private static int DoIncrement(int threadCount, int iterations, Action increment)
    {
        _index = 0;
        var threads = new List<Thread>();
        for (int i = 0; i < threadCount; i++)
        {
            var t = new Thread(() =>
            {
                Event.WaitOne();
                for (int j = 0; j < iterations; j++)
                {
                    increment();
                }
            }) { IsBackground = true };
            threads.Add(t);
            t.Start();
        }

        Event.Set();
        threads.ForEach(t => t.Join());
        return iterations * threadCount;
    }
}