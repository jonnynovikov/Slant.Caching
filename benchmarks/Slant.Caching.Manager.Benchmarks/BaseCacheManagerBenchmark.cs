using BenchmarkDotNet.Attributes;
using Slant.Caching.Manager.Configuration;
using Slant.Caching.Manager.MemoryCache;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Slant.Caching.Manager.Benchmarks
{
    [ExcludeFromCodeCoverage]
    public abstract class BaseCacheBenchmark
    {
        private static ICacheManagerConfiguration BaseConfig
            => new ConfigurationBuilder()
            .WithMaxRetries(10)
            .WithRetryTimeout(500)
            .WithUpdateMode(CacheUpdateMode.Up)
            .Build();

        protected ICacheManager<string> DictionaryCache = new BaseCacheManager<string>(BaseConfig.Builder.WithDictionaryHandle().Build());

        protected ICacheManager<string> MsMemoryCache = new BaseCacheManager<string>(BaseConfig.Builder.WithMicrosoftMemoryCacheHandle().Build());


        [GlobalSetup]
        public void Setup()
        {
            DictionaryCache.Clear();
            MsMemoryCache.Clear();
            SetupBench();
        }

        [Benchmark(Baseline = true)]
        public void Dictionary()
        {
            Execute(DictionaryCache);
        }

        [Benchmark]
        public void MsMemory()
        {
            Execute(MsMemoryCache);
        }

        protected abstract void Execute(ICacheManager<string> cache);

        protected virtual void SetupBench()
        {
        }
    }

    #region add

    [ExcludeFromCodeCoverage]
    public class AddSingleBenchmark : BaseCacheBenchmark
    {
        private string _key = Guid.NewGuid().ToString();

        protected override void Execute(ICacheManager<string> cache)
        {
            if (!cache.Add(_key, "value"))
            {
                cache.Remove(_key);
            }
        }
    }

    [ExcludeFromCodeCoverage]
    public class AddWithRegionSingleBenchmark : BaseCacheBenchmark
    {
        private string _key = Guid.NewGuid().ToString();

        protected override void Execute(ICacheManager<string> cache)
        {
            if (!cache.Add(_key, "value", "region"))
            {
                cache.Remove(_key);
            }
        }
    }

    #endregion add

    #region put

    [ExcludeFromCodeCoverage]
    public class PutSingleBenchmark : BaseCacheBenchmark
    {
        private string _key = Guid.NewGuid().ToString();

        protected override void Execute(ICacheManager<string> cache)
        {
            cache.Put(_key, "value");
        }
    }

    [ExcludeFromCodeCoverage]
    public class PutWithRegionSingleBenchmark : BaseCacheBenchmark
    {
        private string _key = Guid.NewGuid().ToString();

        protected override void Execute(ICacheManager<string> cache)
        {
            cache.Put(_key, "value", "region");
        }
    }

    #endregion put

    #region get

    [ExcludeFromCodeCoverage]
    public class GetSingleBenchmark : BaseCacheBenchmark
    {
        protected string Key = Guid.NewGuid().ToString();

        protected override void Execute(ICacheManager<string> cache)
        {
            var val = cache.Get(Key);
            if (val == null)
            {
                throw new InvalidOperationException();
            }
        }

        protected override void SetupBench()
        {
            base.SetupBench();

            DictionaryCache.Add(Key, Key);
            DictionaryCache.Add(Key, Key, "region");
            MsMemoryCache.Add(Key, Key);
            MsMemoryCache.Add(Key, Key, "region");
        }
    }

    [ExcludeFromCodeCoverage]
    public class GetWithRegionSingleBenchmark : GetSingleBenchmark
    {
        protected override void Execute(ICacheManager<string> cache)
        {
            var val = cache.Get(Key, "region");
            if (val == null)
            {
                throw new InvalidOperationException();
            }
        }
    }

    #endregion get

    #region update

    [ExcludeFromCodeCoverage]
    public class UpdateSingleBenchmark : GetSingleBenchmark
    {
        protected override void Execute(ICacheManager<string> cache)
        {
            var val = cache.Update(Key, (v) => v.Equals("bla") ? "bla" : "blub");
            if (val == null)
            {
                throw new InvalidOperationException();
            }
        }
    }

    #endregion upate
}
