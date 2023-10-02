using System;

namespace Slant.Caching.Manager;

/// <summary>
/// This interface is the base contract for the main stack of this library.
/// <para>
/// The <c>ICacheHandle</c> and <c>ICacheManager</c> interfaces are derived from <c>ICache</c>,
/// meaning the method call signature throughout the stack is very similar.
/// </para>
/// <para>
/// We want the flexibility of having a simple get/put/delete cache up to multiple caches
/// layered on top of each other, still using the same simple and easy to understand interface.
/// </para>
/// <para>
/// The <c>TCacheValue</c> can, but most not be used in the sense of strongly typing. This
/// means, you can define and configure a cache for certain object types within your domain. But
/// you can also use <c>object</c> and store anything you want within the cache. All underlying
/// cache technologies usually do not care about types of the cache items.
/// </para>
/// </summary>
/// <typeparam name="TCacheValue">The type of the cache value.</typeparam>
public interface IRegionCache<TCacheValue> : IDisposable
{
    /// <summary>
    /// Gets or sets a value for the specified key and region. The indexer is identical to the
    /// corresponding <see cref="Put(string, TCacheValue, string)"/> and
    /// <see cref="Get(string, string)"/> calls.
    /// <para>
    /// With <paramref name="region"/> specified, the key will <b>not</b> be found in the global cache.
    /// </para>
    /// </summary>
    /// <param name="key">The key being used to identify the item within the cache.</param>
    /// <param name="region">The cache region.</param>
    /// <returns>
    /// The value being stored in the cache for the given <paramref name="key"/> and <paramref name="region"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// If the <paramref name="key"/> or <paramref name="region"/> is null.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1023:IndexersShouldNotBeMultidimensional", Justification = "nope")]
    TCacheValue this[string key, string region] { get; set; }

    /// <summary>
    /// Adds a value for the specified key and region to the cache.
    /// <para>
    /// The <c>Add</c> method will <b>not</b> be successful if the specified
    /// <paramref name="key"/> already exists within the cache!
    /// </para>
    /// <para>
    /// With <paramref name="region"/> specified, the key will <b>not</b> be found in the global cache.
    /// </para>
    /// </summary>
    /// <param name="key">The key being used to identify the item within the cache.</param>
    /// <param name="value">The value which should be cached.</param>
    /// <param name="region">The cache region.</param>
    /// <returns>
    /// <c>true</c> if the key was not already added to the cache, <c>false</c> otherwise.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// If the <paramref name="key"/>, <paramref name="value"/> or <paramref name="region"/> is null.
    /// </exception>
    bool Add(string key, TCacheValue value, string region);

    /// <summary>
    /// Clears the cache region, removing all items from the specified <paramref name="region"/> only.
    /// </summary>
    /// <param name="region">The cache region.</param>
    /// <exception cref="ArgumentNullException">If the <paramref name="region"/> is null.</exception>
    void ClearRegion(string region);

    /// <summary>
    /// Returns a value indicating if the <paramref name="key"/> in <paramref name="region"/> exists in at least one cache layer
    /// configured in CacheManger, without actually retrieving it from the cache (if supported).
    /// </summary>
    /// <param name="key">The cache key to check.</param>
    /// <param name="region">The cache region.</param>
    /// <returns><c>True</c> if the <paramref name="key"/> exists, <c>False</c> otherwise.</returns>
    bool Exists(string key, string region);

    /// <summary>
    /// Gets a value for the specified key and region.
    /// </summary>
    /// <param name="key">The key being used to identify the item within the cache.</param>
    /// <param name="region">The cache region.</param>
    /// <returns>
    /// The value being stored in the cache for the given <paramref name="key"/> and <paramref name="region"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// If the <paramref name="key"/> or <paramref name="region"/> is null.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords",
        MessageId = "Get", Justification = "Maybe at some point.")]
    TCacheValue? Get(string key, string region);

    /// <summary>
    /// Gets a value for the specified key and region and will cast it to the specified type.
    /// </summary>
    /// <typeparam name="TOut">The type the cached value should be converted to.</typeparam>
    /// <param name="key">The key being used to identify the item within the cache.</param>
    /// <param name="region">The cache region.</param>
    /// <returns>
    /// The value being stored in the cache for the given <paramref name="key"/> and <paramref name="region"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// If the <paramref name="key"/> or <paramref name="region"/> is null.
    /// </exception>
    /// <exception cref="InvalidCastException">
    /// If no explicit cast is defined from <c>TCacheValue</c> to <c>TOut</c>.
    /// </exception>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords",
        MessageId = "Get", Justification = "Maybe at some point.")]
    TOut Get<TOut>(string key, string region);

    /// <summary>
    /// Gets the <c>CacheItem</c> for the specified key and region.
    /// </summary>
    /// <param name="key">The key being used to identify the item within the cache.</param>
    /// <param name="region">The cache region.</param>
    /// <returns>The <c>CacheItem</c>.</returns>
    /// <exception cref="ArgumentNullException">
    /// If the <paramref name="key"/> or <paramref name="region"/> is null.
    /// </exception>
    CacheItem<TCacheValue>? GetCacheItem(string key, string region);

    /// <summary>
    /// Puts a value for the specified key and region into the cache.
    /// <para>
    /// If the <paramref name="key"/> already exists within the cache, the existing value will
    /// be replaced with the new <paramref name="value"/>.
    /// </para>
    /// <para>
    /// With <paramref name="region"/> specified, the key will <b>not</b> be found in the global cache.
    /// </para>
    /// </summary>
    /// <param name="key">The key being used to identify the item within the cache.</param>
    /// <param name="value">The value which should be cached.</param>
    /// <param name="region">The cache region.</param>
    /// <exception cref="ArgumentNullException">
    /// If the <paramref name="key"/>, <paramref name="value"/> or <paramref name="region"/> is null.
    /// </exception>
    void Put(string key, TCacheValue value, string region);

    /// <summary>
    /// Removes a value from the cache for the specified key and region.
    /// </summary>
    /// <param name="key">The key being used to identify the item within the cache.</param>
    /// <param name="region">The cache region.</param>
    /// <returns>
    /// <c>true</c> if the key was found and removed from the cache, <c>false</c> otherwise.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// If the <paramref name="key"/> or <paramref name="region"/> is null.
    /// </exception>
    bool Remove(string key, string region);
}