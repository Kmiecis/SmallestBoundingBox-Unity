using System;
using System.Diagnostics;

public static class TimeMeasure
{
    public static double InSeconds(Action action)
    {
        Stopwatch stopwatch = new Stopwatch();

        stopwatch.Start();
        action.Invoke();
        stopwatch.Stop();

        return stopwatch.Elapsed.TotalSeconds;
    }

    public static double InMilliseconds(Action action)
    {
        Stopwatch stopwatch = new Stopwatch();

        stopwatch.Start();
        action.Invoke();
        stopwatch.Stop();

        return stopwatch.Elapsed.TotalMilliseconds;
    }
}