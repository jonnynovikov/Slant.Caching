using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Slant.Caching.Manager.Internal;
using Slant.Caching.Manager.Logging;
using MemoryCacheImpl = Microsoft.Extensions.Caching.Memory.MemoryCache;
using static Slant.Caching.Manager.Utility.Guard;

namespace Slant.Caching.Manager.MemoryCache;

/// <summary>
/// Implementation of a cache handle using <see cref="Microsoft.Extensions.Caching.Memory"/>.
/// </summary>
/// <typeparam name="TCacheValue">The type of the cache value.</typeparam>
public class MemoryCacheHandle<TCacheValue> : BaseCacheHandle<TCacheValue>
{
    private const string DefaultName = "default";

    private readonly string _cacheName = string.Empty;

    private volatile MemoryCacheImpl _cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryCacheHandle{TCacheValue}"/> class.
    /// </summary>
    /// <param name="managerConfiguration">The manager configuration.</param>
    /// <param name="configuration">The cache handle configuration.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public MemoryCacheHandle(
        ICacheManagerConfiguration managerConfiguration,
        CacheHandleConfiguration configuration,
        ILoggerFactory loggerFactory) : this(managerConfiguration, configuration, loggerFactory, null)
    {
    }
        
    /// <summary>
    /// Initializes a new instance of the <see cref="MemoryCacheHandle{TCacheValue}"/> class.
    /// </summary>
    /// <param name="managerConfiguration">The manager configuration.</param>
    /// <param name="configuration">The cache handle configuration.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <param name="memoryCacheOptions">The vendor specific options.</param>
    public MemoryCacheHandle(
        ICacheManagerConfiguration managerConfiguration, 
        CacheHandleConfiguration configuration, 
        ILoggerFactory loggerFactory, 
        MemoryCacheOptions? memoryCacheOptions)
        : base(managerConfiguration, configuration)
    {
        NotNull(managerConfiguration, nameof(managerConfiguration));
        NotNull(configuration, nameof(configuration));
        NotNull(loggerFactory, nameof(loggerFactory));
            
        _cacheName = configuration.Name!;
        Logger = loggerFactory.CreateLogger(this);
            
        MemoryCacheOptions = memoryCacheOptions ?? new MemoryCacheOptions();
        _cache = new MemoryCacheImpl(MemoryCacheOptions);
    }

    /// <inheritdoc/>
    public override int Count => _cache.Count;

    /// <inheritdoc/>
    protected override ILogger Logger { get; }

    internal MemoryCacheOptions MemoryCacheOptions { get; }

    /// <inheritdoc/>
    public override void Clear()
    {
        _cache = new MemoryCacheImpl(MemoryCacheOptions);
    }

    /// <inheritdoc/>
    public override void ClearRegion(string region)
    {
        _cache.RemoveChilds(region);
        _cache.Remove(region);
    }

    /// <inheritdoc />
    public override bool Exists(string key)
    {
        return _cache.Contains(GetItemKey(key));
    }

    /// <inheritdoc />
    public override bool Exists(string key, string? region)
    {
        NotNullOrWhiteSpace(region, nameof(region));
        return _cache.Contains(GetItemKey(key, region));
    }

    /// <inheritdoc/>
    protected override CacheItem<TCacheValue>? GetCacheItemInternal(string key)
    {
        return GetCacheItemInternal(key, null);
    }

    /// <inheritdoc/>
    protected override CacheItem<TCacheValue>? GetCacheItemInternal(string key, string? region)
    {
        var fullKey = GetItemKey(key, region);
        var item = _cache.Get(fullKey) as CacheItem<TCacheValue>;

        if (item == null)
        {
            return null;
        }

        if (item.IsExpired)
        {
            RemoveInternal(item.Key, item.Region.Value);
            TriggerCacheSpecificRemove(item.Key, item.Region.Value, CacheItemRemovedReason.Expired, item.Value!);
            return null;
        }

        if (item.ExpirationMode == ExpirationMode.Sliding)
        {
            // item = this.GetItemExpiration(item); // done by cache handle already
            _cache.Set(fullKey, item, GetOptions(item));
        }

        return item;
    }

    /// <inheritdoc/>
    protected override bool RemoveInternal(string key)
    {
        var result = _cache.Contains(key);
        if (result)
        {
            _cache.Remove(key);
        }
        return result;
    }

    /// <inheritdoc/>
    protected override bool RemoveInternal(string key, string region)
    {
        var fullKey = GetItemKey(key, region);
        var result = _cache.Contains(fullKey);
        if (result)
        {
            _cache.Remove(fullKey);
        }

        return result;
    }

    /// <inheritdoc/>
    protected override bool AddInternalPrepared(CacheItem<TCacheValue> item)
    {
        var key = GetItemKey(item);

        if (_cache.Contains(key))
        {
            return false;
        }

        var options = GetOptions(item);
        _cache.Set(key, item, options);

        if (item.Region.HasValue)
        {
            _cache.RegisterChild(item.Region, key);
        }

        return true;
    }

    /// <inheritdoc/>
    protected override void PutInternalPrepared(CacheItem<TCacheValue> item)
    {
        var key = GetItemKey(item);

        var options = GetOptions(item);
        _cache.Set(key, item, options);

        if (item.Region.HasValue)
        {
            _cache.RegisterChild(item.Region, key);
        }
    }

    private string GetItemKey(CacheItem<TCacheValue> item) => GetItemKey(item.Key, item?.Region.Value);
        
    private string GetItemKey(string key, string? region = null)
    {
        NotNullOrWhiteSpace(key, nameof(key));

        if (string.IsNullOrWhiteSpace(region))
        {
            return key;
        }

        return region + ":" + key;
    }

    private MemoryCacheEntryOptions GetOptions(CacheItem<TCacheValue> item)
    {
        if (item.Region.HasValue)
        {
            if (!_cache.Contains(item.Region))
            {
                CreateRegionToken(item.Region.Value);
            }
        }

        var options = new MemoryCacheEntryOptions().SetPriority(CacheItemPriority.Normal);
            
        if (item.ExpirationMode == ExpirationMode.Absolute)
        {
            options.SetAbsoluteExpiration(item.ExpirationTimeout);
            options.RegisterPostEvictionCallback(ItemRemoved!, new KeyRegion(item.Key, item.Region));
        }

        if (item.ExpirationMode == ExpirationMode.Sliding)
        {
            options.SetSlidingExpiration(item.ExpirationTimeout);
            options.RegisterPostEvictionCallback(ItemRemoved!, new KeyRegion(item.Key, item.Region));
        }

        item.LastAccessedUtc = DateTime.UtcNow;

        return options;
    }

    private void CreateRegionToken(string region)
    {
        var options = new MemoryCacheEntryOptions
        {
            Priority = CacheItemPriority.Normal,
            AbsoluteExpiration = DateTimeOffset.MaxValue,
            SlidingExpiration = TimeSpan.MaxValue,
        };

        _cache.Set(region, new ConcurrentDictionary<object, bool>(), options);
    }

    private void ItemRemoved(object key, object value, EvictionReason reason, object state)
    {
        var strKey = key as string;
        if (string.IsNullOrWhiteSpace(strKey))
        {
            return;
        }

        // don't trigger stuff on manual remove
        if (reason == EvictionReason.Removed)
        {
            return;
        }

        var keyWithRegion = state as KeyRegion;

        if (keyWithRegion != null)
        {
            if (keyWithRegion.Region.HasValue)
            {
                Stats.OnRemove(keyWithRegion.Region.Value);
            }
            else
            {
                Stats.OnRemove();
            }

            var item = value as CacheItem<TCacheValue>;
            object? originalValue = null;
            if (item != null)
            {
                originalValue = item.Value!;
            }

            if (reason == EvictionReason.Capacity)
            {
                TriggerCacheSpecificRemove(keyWithRegion.Key, keyWithRegion.Region.Value, CacheItemRemovedReason.Evicted, originalValue);
            }
            else if (reason == EvictionReason.Expired)
            {
                TriggerCacheSpecificRemove(keyWithRegion.Key, keyWithRegion.Region.Value, CacheItemRemovedReason.Expired, originalValue);
            }
        }
        else
        {
            Stats.OnRemove();
        }
    }
}