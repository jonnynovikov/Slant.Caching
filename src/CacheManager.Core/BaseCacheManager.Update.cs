using System;
using System.Globalization;
using System.Threading.Tasks;
using CacheManager.Core.Internal;
using CacheManager.Core.Logging;
using static CacheManager.Core.Utility.Guard;

namespace CacheManager.Core;

public partial class BaseCacheManager<TCacheValue>
{
    /// <inheritdoc />
    public async Task<TCacheValue> AddOrUpdate(string key, TCacheValue addValue, Func<TCacheValue, TCacheValue> updateValue) =>
        await AddOrUpdate(key, addValue, updateValue, Configuration.MaxRetries).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<TCacheValue> AddOrUpdate(string key, string region, TCacheValue addValue, Func<TCacheValue, TCacheValue> updateValue) =>
        await AddOrUpdate(key, region, addValue, updateValue, Configuration.MaxRetries).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<TCacheValue> AddOrUpdate(string key, TCacheValue addValue, Func<TCacheValue, TCacheValue> updateValue, int maxRetries) =>
        await AddOrUpdate(new CacheItem<TCacheValue>(key, addValue), updateValue, maxRetries).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<TCacheValue> AddOrUpdate(string key, string region, TCacheValue addValue, Func<TCacheValue, TCacheValue> updateValue, int maxRetries) =>
        await AddOrUpdate(new CacheItem<TCacheValue>(key, region, addValue), updateValue, maxRetries).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<TCacheValue> AddOrUpdate(CacheItem<TCacheValue> addItem, Func<TCacheValue, TCacheValue> updateValue) =>
        await AddOrUpdate(addItem, updateValue, Configuration.MaxRetries).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<TCacheValue> AddOrUpdate(CacheItem<TCacheValue> addItem, Func<TCacheValue, TCacheValue> updateValue, int maxRetries)
    {
        NotNull(addItem, nameof(addItem));
        NotNull(updateValue, nameof(updateValue));
        Ensure(maxRetries >= 0, "Maximum number of retries must be greater than or equal to zero.");

        return await AddOrUpdateInternal(addItem, updateValue, maxRetries).ConfigureAwait(false);
    }

    private async Task<TCacheValue> AddOrUpdateInternal(CacheItem<TCacheValue> item, Func<TCacheValue, TCacheValue> updateValue, int maxRetries)
    {
        CheckDisposed();
        if (_logTrace)
        {
            Logger.LogTrace("Add or update: {0} {1}.", item.Key, item.Region);
        }

        var tries = 0;
        do
        {
            tries++;

            if (await AddInternal(item).ConfigureAwait(false))
            {
                if (_logTrace)
                {
                    Logger.LogTrace("Add or update: {0} {1}: successfully added the item.", item.Key, item.Region);
                }

                return item.Value;
            }

            if (_logTrace)
            {
                Logger.LogTrace(
                    "Add or update: {0} {1}: add failed, trying to update...",
                    item.Key,
                    item.Region);
            }

                
            var updateResult = string.IsNullOrWhiteSpace(item.Region) ?
                await TryUpdate(item.Key, updateValue, maxRetries).ConfigureAwait(false) :
                await TryUpdate(item.Key, item.Region, updateValue, maxRetries).ConfigureAwait(false);
            bool updated = updateResult.Success;
            if (updated)
            {
                if (_logTrace)
                {
                    Logger.LogTrace("Add or update: {0} {1}: successfully updated.", item.Key, item.Region);
                }

                return updateResult.Item;
            }

            if (_logTrace)
            {
                Logger.LogTrace(
                    "Add or update: {0} {1}: update FAILED, retrying [{2}/{3}].",
                    item.Key,
                    item.Region,
                    tries,
                    Configuration.MaxRetries);
            }
        }
        while (tries <= maxRetries);

        // exceeded max retries, failing the operation... (should not happen in 99,99% of the cases though, better throw?)
        throw new InvalidOperationException(
            string.Format(CultureInfo.InvariantCulture, "Could not add or update the item {0} {1}.", item.Key, item.Region));
    }

    /// <inheritdoc />
    public async Task<(bool Success, TCacheValue Item)> TryUpdate(string key, Func<TCacheValue, TCacheValue> updateValue) =>
        await TryUpdate(key, updateValue, Configuration.MaxRetries).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<(bool Success, TCacheValue Item)> TryUpdate(string key, string region, Func<TCacheValue, TCacheValue> updateValue) =>
        await TryUpdate(key, region, updateValue, Configuration.MaxRetries).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<(bool Success, TCacheValue Item)> TryUpdate(string key, Func<TCacheValue, TCacheValue> updateValue, int maxRetries)
    {
        NotNullOrWhiteSpace(key, nameof(key));
        NotNull(updateValue, nameof(updateValue));
        Ensure(maxRetries >= 0, "Maximum number of retries must be greater than or equal to zero.");

        return await UpdateInternal(_cacheHandles, key, updateValue, maxRetries, false).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<(bool Success, TCacheValue Item)> TryUpdate(string key, string region, Func<TCacheValue, TCacheValue> updateValue, int maxRetries)
    {
        NotNullOrWhiteSpace(key, nameof(key));
        NotNullOrWhiteSpace(region, nameof(region));
        NotNull(updateValue, nameof(updateValue));
        Ensure(maxRetries >= 0, "Maximum number of retries must be greater than or equal to zero.");

        return await UpdateInternal(_cacheHandles, key, region, updateValue, maxRetries, false).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<TCacheValue> Update(string key, Func<TCacheValue, TCacheValue> updateValue) =>
        await Update(key, updateValue, Configuration.MaxRetries).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<TCacheValue> Update(string key, string region, Func<TCacheValue, TCacheValue> updateValue) =>
        await Update(key, region, updateValue, Configuration.MaxRetries).ConfigureAwait(false);

    /// <inheritdoc />
    public async Task<TCacheValue> Update(string key, Func<TCacheValue, TCacheValue> updateValue, int maxRetries)
    {
        NotNullOrWhiteSpace(key, nameof(key));
        NotNull(updateValue, nameof(updateValue));
        Ensure(maxRetries >= 0, "Maximum number of retries must be greater than or equal to zero.");
        var updateResult = await UpdateInternal(_cacheHandles, key, updateValue, maxRetries, true).ConfigureAwait(false);

        if (!updateResult.success)
        {
            throw new InvalidOperationException($"Update failed for key '{key}'.");
        }

        return updateResult.value;
    }

    /// <inheritdoc />
    public async Task<TCacheValue> Update(string key, string region, Func<TCacheValue, TCacheValue> updateValue, int maxRetries)
    {
        NotNullOrWhiteSpace(key, nameof(key));
        NotNullOrWhiteSpace(region, nameof(region));
        NotNull(updateValue, nameof(updateValue));
        Ensure(maxRetries >= 0, "Maximum number of retries must be greater than or equal to zero.");
        var updateResult = await UpdateInternal(_cacheHandles, key, region, updateValue, maxRetries, true).ConfigureAwait(false);
        if (!updateResult.success)
        {
            throw new InvalidOperationException($"Update failed for key '{region}:{key}'.");
        }

        return updateResult.value;
    }

    private async Task<(bool success, TCacheValue value)> UpdateInternal(
        BaseCacheHandle<TCacheValue>[] handles,
        string key,
        Func<TCacheValue, TCacheValue> updateValue,
        int maxRetries,
        bool throwOnFailure) =>
        await UpdateInternal(handles, key, null, updateValue, maxRetries, throwOnFailure).ConfigureAwait(false);

    private async Task<(bool success, TCacheValue value)> UpdateInternal(
        BaseCacheHandle<TCacheValue>[] handles,
        string key,
        string region,
        Func<TCacheValue, TCacheValue> updateValue,
        int maxRetries,
        bool throwOnFailure)
    {
        CheckDisposed();

        // assign null
        TCacheValue value = default(TCacheValue);

        if (handles.Length == 0)
        {
            return (false, value);
        }

        if (_logTrace)
        {
            Logger.LogTrace("Update: {0} {1}.", key, region);
        }

        // lowest level
        // todo: maybe check for only run on the backplate if configured (could potentially be not the last one).
        var handleIndex = handles.Length - 1;
        var handle = handles[handleIndex];

        var result = string.IsNullOrWhiteSpace(region) ?
            await handle.Update(key, updateValue, maxRetries).ConfigureAwait(false) :
            await handle.Update(key, region, updateValue, maxRetries).ConfigureAwait(false);

        if (_logTrace)
        {
            Logger.LogTrace(
                "Update: {0} {1}: tried on handle {2}: result: {3}.",
                key,
                region,
                handle.Configuration.Name,
                result.UpdateState);
        }

        if (result.UpdateState == UpdateItemResultState.Success)
        {
            // only on success, the returned value will not be null
            value = result.Value.Value;
            handle.Stats.OnUpdate(key, region, result);

            // evict others, we don't know if the update on other handles could actually
            // succeed... There is a risk the update on other handles could create a
            // different version than we created with the first successful update... we can
            // safely add the item to handles below us though.
            await EvictFromHandlesAbove(key, region, handleIndex).ConfigureAwait(false);

            // optimizing - not getting the item again from cache. We already have it
            // var item = string.IsNullOrWhiteSpace(region) ? handle.GetCacheItem(key) : handle.GetCacheItem(key, region);
            await AddToHandlesBelow(result.Value, handleIndex).ConfigureAwait(false);
            TriggerOnUpdate(key, region);
        }
        else if (result.UpdateState == UpdateItemResultState.FactoryReturnedNull)
        {
            Logger.LogWarn($"Update failed on '{region}:{key}' because value factory returned null.");

            if (throwOnFailure)
            {
                throw new InvalidOperationException($"Update failed on '{region}:{key}' because value factory returned null.");
            }
        }
        else if (result.UpdateState == UpdateItemResultState.TooManyRetries)
        {
            // if we had too many retries, this basically indicates an
            // invalid state of the cache: The item is there, but we couldn't update it and
            // it most likely has a different version
            Logger.LogWarn($"Update failed on '{region}:{key}' because of too many retries.");

            await EvictFromOtherHandles(key, region, handleIndex).ConfigureAwait(false);

            if (throwOnFailure)
            {
                throw new InvalidOperationException($"Update failed on '{region}:{key}' because of too many retries: {result.NumberOfTriesNeeded}.");
            }
        }
        else if (result.UpdateState == UpdateItemResultState.ItemDidNotExist)
        {
            // If update fails because item doesn't exist AND the current handle is backplane source or the lowest cache handle level,
            // remove the item from other handles (if exists).
            // Otherwise, if we do not exit here, calling update on the next handle might succeed and would return a misleading result.
            Logger.LogInfo($"Update failed on '{region}:{key}' because the region/key did not exist.");

            await EvictFromOtherHandles(key, region, handleIndex).ConfigureAwait(false);

            if (throwOnFailure)
            {
                throw new InvalidOperationException($"Update failed on '{region}:{key}' because the region/key did not exist.");
            }
        }

        // update backplane
        if (result.UpdateState == UpdateItemResultState.Success && _cacheBackplane != null)
        {
            if (_logTrace)
            {
                Logger.LogTrace("Update: {0} {1}: notifies backplane [change].", key, region);
            }

            if (string.IsNullOrWhiteSpace(region))
            {
                await _cacheBackplane.NotifyChange(key, CacheItemChangedEventAction.Update).ConfigureAwait(false);
            }
            else
            {
                await _cacheBackplane.NotifyChange(key, region, CacheItemChangedEventAction.Update).ConfigureAwait(false);
            }
        }

        return (result.UpdateState == UpdateItemResultState.Success, value);
    }
}