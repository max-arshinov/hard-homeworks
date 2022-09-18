using System;
using System.Diagnostics;

namespace Hw3.Tests;

public class StopWatcher
{
    public static TimeSpan Stopwatch(Action action)
    {
        var sw = new Stopwatch();
        sw.Start();
        action();
        sw.Stop();
        return sw.Elapsed;
    }
}