using System.Threading;

namespace Slant.Caching.Manager.Internal;

internal sealed class CacheStatsCounter
{
    private volatile long[] _counters = new long[9];

    public void Add(CacheStatsCounterType type, long value)
    {
        Interlocked.Add(ref _counters[(int) type], value);
    }

    public void Decrement(CacheStatsCounterType type)
    {
        Interlocked.Decrement(ref _counters[(int) type]);
    }

    public long Get(CacheStatsCounterType type)
    {
        return _counters[(int) type];
    }

    public void Increment(CacheStatsCounterType type)
    {
        Interlocked.Increment(ref _counters[(int) type]);
    }

    public void Set(CacheStatsCounterType type, long value)
    {
        Interlocked.Exchange(ref _counters[(int) type], value);
    }
}