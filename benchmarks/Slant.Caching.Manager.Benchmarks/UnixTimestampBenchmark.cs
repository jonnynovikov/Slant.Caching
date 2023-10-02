using BenchmarkDotNet.Attributes;
using Slant.Caching.Manager.Utility;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Slant.Caching.Manager.Benchmarks
{
    [ExcludeFromCodeCoverage]
    public class UnixTimestampBenchmark
    {
        [Benchmark(Baseline = true)]
        public long Framework()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        private static readonly DateTime _date1970 = new DateTime(1970, 1, 1);

        [Benchmark()]
        public long ManualCalcNaive()
        {
            return (long)(DateTime.UtcNow - _date1970).TotalMilliseconds;
        }

        [Benchmark()]
        public long ManualCalcOptimized()
        {
            return Clock.GetUnixTimestampMillis();
        }
    }
}
