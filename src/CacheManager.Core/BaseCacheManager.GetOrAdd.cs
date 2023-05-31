using System;
using System.Linq;
using System.Threading.Tasks;
using static CacheManager.Core.Utility.Guard;

namespace CacheManager.Core
{
    public partial class BaseCacheManager<TCacheValue>
    {
        /// <inheritdoc />
        public async Task<TCacheValue> GetOrAdd(string key, TCacheValue value)
            => await GetOrAdd(key, (k) => value);

        /// <inheritdoc />
        public async Task<TCacheValue> GetOrAdd(string key, string region, TCacheValue value)
            => await GetOrAdd(key, region, (k, r) => value);

        /// <inheritdoc />
        public async Task<TCacheValue> GetOrAdd(string key, Func<string, TCacheValue> valueFactory)
        {
            NotNullOrWhiteSpace(key, nameof(key));
            NotNull(valueFactory, nameof(valueFactory));

            return (await GetOrAddInternal(key, null, (k, r) => new CacheItem<TCacheValue>(k, valueFactory(k)))).Value;
        }

        /// <inheritdoc />
        public async Task<TCacheValue> GetOrAdd(string key, string region, Func<string, string, TCacheValue> valueFactory)
        {
            NotNullOrWhiteSpace(key, nameof(key));
            NotNullOrWhiteSpace(region, nameof(region));
            NotNull(valueFactory, nameof(valueFactory));

            return (await GetOrAddInternal(key, region, (k, r) => new CacheItem<TCacheValue>(k, r, valueFactory(k, r)))).Value;
        }

        /// <inheritdoc />
        public async Task<CacheItem<TCacheValue>> GetOrAdd(string key, Func<string, CacheItem<TCacheValue>> valueFactory)
        {
            NotNullOrWhiteSpace(key, nameof(key));
            NotNull(valueFactory, nameof(valueFactory));

            return await GetOrAddInternal(key, null, (k, r) => valueFactory(k));
        }

        /// <inheritdoc />
        public async Task<CacheItem<TCacheValue>> GetOrAdd(string key, string region, Func<string, string, CacheItem<TCacheValue>> valueFactory)
        {
            NotNullOrWhiteSpace(key, nameof(key));
            NotNullOrWhiteSpace(region, nameof(region));
            NotNull(valueFactory, nameof(valueFactory));

            return await GetOrAddInternal(key, region, valueFactory);
        }

        /// <inheritdoc />
        public async Task<(bool Success, TCacheValue Item)> TryGetOrAdd(string key, Func<string, TCacheValue> valueFactory)
        {
            NotNullOrWhiteSpace(key, nameof(key));
            NotNull(valueFactory, nameof(valueFactory));

            var getOrAddResult = await TryGetOrAddInternal(
                key,
                null,
                (k, r) =>
                {
                    var newValue = valueFactory(k);
                    return newValue == null ? null : new CacheItem<TCacheValue>(k, newValue);
                });

            if (getOrAddResult.Success)
                return (getOrAddResult.Success, getOrAddResult.Item.Value);

            return (false, default(TCacheValue));

        }

        /// <inheritdoc />
        public async Task<(bool Success, TCacheValue Item)> TryGetOrAdd(string key, string region, Func<string, string, TCacheValue> valueFactory)
        {
            NotNullOrWhiteSpace(key, nameof(key));
            NotNullOrWhiteSpace(region, nameof(region));
            NotNull(valueFactory, nameof(valueFactory));

            var getOrAddResult = await TryGetOrAddInternal(
                key,
                region,
                (k, r) =>
                {
                    var newValue = valueFactory(k, r);
                    return newValue == null ? null : new CacheItem<TCacheValue>(k, r, newValue);
                });

            if (getOrAddResult.Success)
                return (getOrAddResult.Success, getOrAddResult.Item.Value);
            
            return (false, default(TCacheValue));
        }

        /// <inheritdoc />
        public async Task<(bool Success, CacheItem<TCacheValue> Item)> TryGetOrAdd(string key, Func<string, CacheItem<TCacheValue>> valueFactory)
        {
            NotNullOrWhiteSpace(key, nameof(key));
            NotNull(valueFactory, nameof(valueFactory));

            return await TryGetOrAddInternal(key, null, (k, r) => valueFactory(k));
        }

        /// <inheritdoc />
        public async Task<(bool Success, CacheItem<TCacheValue> Item)> TryGetOrAdd(string key, string region, Func<string, string, CacheItem<TCacheValue>> valueFactory)
        {
            NotNullOrWhiteSpace(key, nameof(key));
            NotNullOrWhiteSpace(region, nameof(region));
            NotNull(valueFactory, nameof(valueFactory));

            return await TryGetOrAddInternal(key, region, valueFactory);
        }

        private async Task<(bool Success, CacheItem<TCacheValue> Item)> TryGetOrAddInternal(string key, string region, Func<string, string, CacheItem<TCacheValue>> valueFactory)
        {
            CacheItem<TCacheValue> item = null;
            CacheItem<TCacheValue> newItem = null;
            var tries = 0;
            do
            {
                tries++;
                item = await GetCacheItemInternal(key, region);
                if (item != null)
                {
                    return (true, item);
                }

                // changed logic to invoke the factory only once in case of retries
                if (newItem == null)
                {
                    newItem = valueFactory(key, region);
                }

                if (newItem == null)
                {
                    return (false, item);
                }

                if (await AddInternal(newItem))
                {
                    item = newItem;
                    return (true, item);
                }
            }
            while (tries <= Configuration.MaxRetries);

            return (false, item);
        }

        private async Task<CacheItem<TCacheValue>> GetOrAddInternal(string key, string region, Func<string, string, CacheItem<TCacheValue>> valueFactory)
        {
            CacheItem<TCacheValue> newItem = null;
            var tries = 0;
            do
            {
                tries++;
                var item = await GetCacheItemInternal(key, region);
                if (item != null)
                {
                    return item;
                }

                // changed logic to invoke the factory only once in case of retries
                if (newItem == null)
                {
                    newItem = valueFactory(key, region);
                }

                // Throw explicit to me more consistent. Otherwise it would throw later eventually...
                if (newItem == null)
                {
                    throw new InvalidOperationException("The CacheItem which should be added must not be null.");
                }

                if (await AddInternal(newItem))
                {
                    return newItem;
                }
            }
            while (tries <= Configuration.MaxRetries);

            // should usually never occur, but could if e.g. max retries is 1 and an item gets added between the get and add.
            // pretty unusual, so keep the max tries at least around 50
            throw new InvalidOperationException(
                string.Format("Could not get nor add the item {0} {1}", key, region));
        }
    }
}
