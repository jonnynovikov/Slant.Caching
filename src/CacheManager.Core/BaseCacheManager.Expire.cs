using System;
using System.Linq;
using System.Threading.Tasks;
using CacheManager.Core.Logging;

namespace CacheManager.Core
{
    public partial class BaseCacheManager<TCacheValue>
    {
        /// <inheritdoc />
        public async Task Expire(string key, ExpirationMode mode, TimeSpan timeout)
            => await ExpireInternal(key, null, mode, timeout);

        /// <inheritdoc />
        public async Task Expire(string key, string region, ExpirationMode mode, TimeSpan timeout)
            => await ExpireInternal(key, region, mode, timeout);

        private async Task ExpireInternal(string key, string region, ExpirationMode mode, TimeSpan timeout)
        {
            CheckDisposed();

            var item = await GetCacheItemInternal(key, region);
            if (item == null)
            {
                Logger.LogTrace("Expire: item not found for key {0}:{1}", key, region);
                return;
            }

            if (_logTrace)
            {
                Logger.LogTrace("Expire [{0}] started.", item);
            }

            if (mode == ExpirationMode.Absolute)
            {
                item = item.WithAbsoluteExpiration(timeout);
            }
            else if (mode == ExpirationMode.Sliding)
            {
                item = item.WithSlidingExpiration(timeout);
            }
            else if (mode == ExpirationMode.None)
            {
                item = item.WithNoExpiration();
            }
            else if (mode == ExpirationMode.Default)
            {
                item = item.WithDefaultExpiration();
            }

            if (_logTrace)
            {
                Logger.LogTrace("Expire - Expiration of [{0}] has been modified. Using put to store the item...", item);
            }

            await PutInternal(item);
        }

        /// <inheritdoc />
        public async Task Expire(string key, DateTimeOffset absoluteExpiration)
        {
            var timeout = absoluteExpiration.UtcDateTime - DateTime.UtcNow;
            if (timeout <= TimeSpan.Zero)
            {
                throw new ArgumentException("Expiration value must be greater than zero.", nameof(absoluteExpiration));
            }

            await Expire(key, ExpirationMode.Absolute, timeout);
        }

        /// <inheritdoc />
        public async Task Expire(string key, string region, DateTimeOffset absoluteExpiration)
        {
            var timeout = absoluteExpiration.UtcDateTime - DateTime.UtcNow;
            if (timeout <= TimeSpan.Zero)
            {
                throw new ArgumentException("Expiration value must be greater than zero.", nameof(absoluteExpiration));
            }

            await Expire(key, region, ExpirationMode.Absolute, timeout);
        }

        /// <inheritdoc />
        public async Task Expire(string key, TimeSpan slidingExpiration)
        {
            if (slidingExpiration <= TimeSpan.Zero)
            {
                throw new ArgumentException("Expiration value must be greater than zero.", nameof(slidingExpiration));
            }

            await Expire(key, ExpirationMode.Sliding, slidingExpiration);
        }

        /// <inheritdoc />
        public async Task Expire(string key, string region, TimeSpan slidingExpiration)
        {
            if (slidingExpiration <= TimeSpan.Zero)
            {
                throw new ArgumentException("Expiration value must be greater than zero.", nameof(slidingExpiration));
            }

            await Expire(key, region, ExpirationMode.Sliding, slidingExpiration);
        }

        /// <inheritdoc />
        public async Task RemoveExpiration(string key)
        {
            await Expire(key, ExpirationMode.None, default(TimeSpan));
        }

        /// <inheritdoc />
        public async Task RemoveExpiration(string key, string region)
        {
            await Expire(key, region, ExpirationMode.None, default(TimeSpan));
        }
    }
}
