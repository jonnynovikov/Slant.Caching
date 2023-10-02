using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Slant.Caching.Manager.Internal;
using FluentAssertions;
using Slant.Caching.Manager.Configuration;
using Xunit;

namespace Slant.Caching.Manager.Tests;

[ExcludeFromCodeCoverage]
public class ValidConfigurationValidationTests
{
    [Fact]
    public void Cfg_Valid_AppConfig_ByNameByLoader()
    {
        // arrange
        var fileName = TestConfigurationHelper.GetCfgFileName(@"/App.config");
        var cacheName = "C1";

        // act
        var cfg = ConfigurationBuilder.LoadConfigurationFile(fileName, cacheName);
        var cache = CacheFactory.FromConfiguration<object>(cfg);

        // assert
        cache.Configuration.UpdateMode.Should().Be(CacheUpdateMode.Up);
        cache.CacheHandles.Count().Should().Be(3);
        AssertCacheHandleConfig(cache.CacheHandles.ElementAt(0), "h1", ExpirationMode.None, new TimeSpan(0, 0, 50));
        AssertCacheHandleConfig(cache.CacheHandles.ElementAt(1), "h2", ExpirationMode.Absolute, new TimeSpan(0, 20, 0));
        AssertCacheHandleConfig(cache.CacheHandles.ElementAt(2), "h3", ExpirationMode.Sliding, new TimeSpan(20, 0, 0));
    }

    [Fact]
    public void Cfg_Valid_AppConfig_ByName()
    {
        // arrange
        var fileName = TestConfigurationHelper.GetCfgFileName(@"/App.config");
        var cacheName = "C1";

        // act
        var cfg = ConfigurationBuilder.LoadConfigurationFile(fileName, cacheName);
        var cache = CacheFactory.FromConfiguration<object>(cfg);

        // assert
        cache.Configuration.UpdateMode.Should().Be(CacheUpdateMode.Up);
        cache.CacheHandles.Count().Should().Be(3);
        AssertCacheHandleConfig(cache.CacheHandles.ElementAt(0), "h1", ExpirationMode.None, new TimeSpan(0, 0, 50));
        AssertCacheHandleConfig(cache.CacheHandles.ElementAt(1), "h2", ExpirationMode.Absolute, new TimeSpan(0, 20, 0));
        AssertCacheHandleConfig(cache.CacheHandles.ElementAt(2), "h3", ExpirationMode.Sliding, new TimeSpan(20, 0, 0));
    }

    [Fact]
    public void Cfg_Valid_CfgFile_ExpirationVariances()
    {
        // arrange
        var fileName = TestConfigurationHelper.GetCfgFileName(@"/Configuration/configuration.valid.allFeatures.config");
        var cacheName = "ExpirationVariances";

        // act
        var cfg = ConfigurationBuilder.LoadConfigurationFile(fileName, cacheName);
        var cache = CacheFactory.FromConfiguration<object>(cfg);

        // assert
        cache.Configuration.UpdateMode.Should().Be(CacheUpdateMode.Up);
        cache.CacheHandles.Count().Should().Be(4);
        AssertCacheHandleConfig(cache.CacheHandles.ElementAt(0), "h1", ExpirationMode.None, new TimeSpan(0, 0, 50));
        AssertCacheHandleConfig(cache.CacheHandles.ElementAt(1), "h2", ExpirationMode.Sliding, new TimeSpan(0, 5, 0));
        AssertCacheHandleConfig(cache.CacheHandles.ElementAt(2), "h3", ExpirationMode.None, new TimeSpan(0, 0, 0));
        AssertCacheHandleConfig(cache.CacheHandles.ElementAt(3), "h4", ExpirationMode.Absolute, new TimeSpan(0, 20, 0));
    }

    [Fact]
    public void Cfg_Valid_CfgFile_EnabledStats()
    {
        // arrange
        var fileName = TestConfigurationHelper.GetCfgFileName(@"/Configuration/configuration.valid.allFeatures.config");
        var cacheName = "ExpirationVariances";

        // act
        var cfg = ConfigurationBuilder.LoadConfigurationFile(fileName, cacheName);
        var cache = CacheFactory.FromConfiguration<object>(cfg);

        // assert
        cache.CacheHandles.Select(p => p.Configuration.EnableStatistics)
            .Should().AllBeEquivalentTo(true);
    }

    [Fact]
    public void Cfg_Valid_CfgFile_DisableStats()
    {
        // arrange
        var fileName = TestConfigurationHelper.GetCfgFileName(@"/Configuration/configuration.valid.allFeatures.config");
        var cacheName = "c3";

        // act
        var cfg = ConfigurationBuilder.LoadConfigurationFile(fileName, cacheName);
        var cache = CacheFactory.FromConfiguration<object>(cfg);

        // assert
        cache.CacheHandles.Select(p => p.Configuration.EnableStatistics)
            .Should().AllBeEquivalentTo(false);
    }

    [Fact]
    [Trait("category", "NotOnMono")]
    public void Cfg_Valid_CfgFile_AllDefaults()
    {
        // arrange
        var fileName = TestConfigurationHelper.GetCfgFileName(@"/Configuration/configuration.valid.allFeatures.config");
        var cacheName = "onlyDefaultsCache";

        // act
        var cfg = ConfigurationBuilder.LoadConfigurationFile(fileName, cacheName);
        var cache = CacheFactory.FromConfiguration<string>(cfg);

        // assert
        cache.Configuration.UpdateMode.Should().Be(CacheUpdateMode.Up);
        cache.Configuration.SerializerType.Should().BeNull();
        cache.Configuration.LoggerFactoryType.Should().BeNull();
        cache.Configuration.BackplaneType.Should().BeNull();
        cache.Configuration.RetryTimeout.Should().Be(100);
        cache.Configuration.MaxRetries.Should().Be(50);
        cache.CacheHandles.Count().Should().Be(1);
        AssertCacheHandleConfig(cache.CacheHandles.ElementAt(0), "defaultsHandle", ExpirationMode.None, TimeSpan.Zero);
    }

    private static void AssertCacheHandleConfig<T>(
        BaseCacheHandle<T> handle, 
        string name, 
        ExpirationMode mode,
        TimeSpan timeout)
    {
        var cfg = handle.Configuration;
        cfg.Name.Should().Be(name);
        cfg.ExpirationMode.Should().Be(mode);
        cfg.ExpirationTimeout.Should().Be(timeout);
    }
}