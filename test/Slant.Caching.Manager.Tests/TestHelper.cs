﻿using Slant.Caching.Manager.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Slant.Caching.Manager.Tests;

public static class TestHelper
{
    static internal class Loggers
    {
        internal class NoneLogger : ILogger
        {
            public IDisposable BeginScope(object state)
            {
                return null;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return false;
            }

            public void Log(LogLevel logLevel, int eventId, object message, Exception? exception)
            {
            }
        }

        public static ILogger None => new NoneLogger();
    }
    
    
    public static async Task WaitUntilCancel(Action<CancellationTokenSource> act, int timeoutInMillis = 5000)
    {
        var source = new CancellationTokenSource();
        act(source);
        try
        {
            await Task.Delay(timeoutInMillis, source.Token);
        }
        catch
        {
            // do nothing
        }
    }

    /// <summary>
    /// Retries the <paramref name="action"/> for a maximum of <paramref name="tries"/> or until <paramref name="condition"/> returns <c>True</c>.
    /// </summary>
    /// <param name="tries">Number of tries.</param>
    /// <param name="action">The action to retry.</param>
    /// <param name="condition">The condition to signal to stop retrying.</param>
    public static void RetryWithCondition(int tries, Action action, Func<bool> condition)
    {
        var currentTry = 0;
        while (currentTry < tries)
        {
            currentTry++;
            Console.WriteLine("RetryWithCondition try " + currentTry);
            action();
            if (condition())
            {
                Console.WriteLine("RetryWithCondition break for condition after try " + currentTry);
                break;
            }
        }
    }

    /// <summary>
    /// Retries the <paramref name="action"/> for a maximum of <paramref name="tries"/> or until <paramref name="condition"/> returns <c>True</c>.
    /// </summary>
    /// <param name="tries">Number of tries.</param>
    /// <param name="action">The action to retry.</param>
    /// <param name="condition">The condition to signal to stop retrying.</param>
    public static async Task RetryWithCondition(int tries, Func<Task> action, Func<bool> condition)
    {
        var currentTry = 0;
        while (currentTry < tries)
        {
            currentTry++;
            Console.WriteLine("RetryWithCondition try " + currentTry);
            await action();
            if (condition())
            {
                Console.WriteLine("RetryWithCondition break for condition after try " + currentTry);
                break;
            }
        }
    }
}