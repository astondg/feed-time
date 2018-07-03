namespace FeedTime.Common.DataModel
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using FeedTime.Common.Extensions;
    using Windows.Foundation;
    using Windows.Storage;

    public sealed class CacheRetriever
    {
        // TODO - Replace with Mutex for cross process synchronisation
        private static readonly SemaphoreSlim activityScheduleSemaphore = new SemaphoreSlim(1);
        private static readonly SemaphoreSlim dataTrendsSemaphore = new SemaphoreSlim(1);
        private Lazy<Task<IStorageFile>> activityScheduleCacheFile;
        private Lazy<Task<Dictionary<string, ActivitySchedule>>> activityScheduleCache;
        private Lazy<Task<IStorageFile>> dataTrendsCacheFile;
        private Lazy<Task<Dictionary<string, DataTrends>>> dataTrendsCache;

        public CacheRetriever()
        {
            ExpireCache();
        }

        public IAsyncOperation<ActivitySchedule> RetrieveActivitySchedule(string babyId)
        {
            return RetrieveActivityScheduleFromCache(babyId).AsAsyncOperation();
        }

        public IAsyncAction SaveActivitySchedule(ActivitySchedule schedule, string babyId)
        {
            return SaveActivityScheduleToCache(schedule, babyId).AsAsyncAction();
        }

        public void ExpireCache()
        {
            activityScheduleCacheFile = new Lazy<Task<IStorageFile>>(RetrieveActivityScheduleCacheFile);
            activityScheduleCache = new Lazy<Task<Dictionary<string, ActivitySchedule>>>(RetrieveActivityScheduleCache);
            dataTrendsCacheFile = new Lazy<Task<IStorageFile>>(RetrieveDataTrendsCacheFile);
            dataTrendsCache = new Lazy<Task<Dictionary<string, DataTrends>>>(RetrieveDataTrendsCache);
        }

        private async Task<ActivitySchedule> RetrieveActivityScheduleFromCache(string babyId)
        {
            ActivitySchedule activitySchedule = null;
            var cache = await activityScheduleCache.Value;
            cache.TryGetValue(babyId, out activitySchedule);
            return activitySchedule;
        }

        private async Task SaveActivityScheduleToCache(ActivitySchedule schedule, string babyId)
        {
            var cache = await activityScheduleCache.Value;
            cache[babyId] = schedule;
            var cacheFile = await activityScheduleCacheFile.Value;
            await activityScheduleSemaphore.WaitAsync();
            try
            {
                await cacheFile.WriteJsonToFile(typeof(Dictionary<string, ActivitySchedule>),
                                                cache);
            }
            finally
            {
                activityScheduleSemaphore.Release();
            }
        }

        private async Task<IStorageFile> RetrieveActivityScheduleCacheFile()
        {
            return await ApplicationData.Current
                                        .LocalFolder
                                        .CreateFileAsync("_activitySchedule.json",
                                                         CreationCollisionOption.OpenIfExists);
        }

        private async Task<Dictionary<string, ActivitySchedule>> RetrieveActivityScheduleCache()
        {
            var file = await activityScheduleCacheFile.Value;
            await activityScheduleSemaphore.WaitAsync();
            try
            {
                var cache = (Dictionary<string, ActivitySchedule>)await file.ReadJsonFileAs(typeof(Dictionary<string, ActivitySchedule>));
                return cache ?? new Dictionary<String, ActivitySchedule>();
            }
            finally
            {
                activityScheduleSemaphore.Release();
            }
        }

        public IAsyncOperation<DataTrends> RetrieveDataTrends(string babyId)
        {
            return RetrieveDataTrendsFromCache(babyId).AsAsyncOperation();
        }

        public IAsyncAction SaveDataTrends(DataTrends latestTrends, string babyId)
        {
            return SaveDataTrendsToCache(latestTrends, babyId).AsAsyncAction();
        }

        private async Task<DataTrends> RetrieveDataTrendsFromCache(string babyId)
        {
            DataTrends latestTrends = null;
            var cache = await dataTrendsCache.Value;
            cache.TryGetValue(babyId, out latestTrends);
            return latestTrends;
        }

        private async Task SaveDataTrendsToCache(DataTrends latestTrends, string babyId)
        {
            var cache = await dataTrendsCache.Value;
            cache[babyId] = latestTrends;
            var cacheFile = await dataTrendsCacheFile.Value;
            await dataTrendsSemaphore.WaitAsync();
            try
            {
                await cacheFile.WriteJsonToFile(typeof(Dictionary<string, DataTrends>),
                                                cache);
            }
            finally
            {
                dataTrendsSemaphore.Release();
            }
        }

        private async Task<IStorageFile> RetrieveDataTrendsCacheFile()
        {
            return await ApplicationData.Current
                                        .LocalFolder
                                        .CreateFileAsync("_dataTrends.json",
                                                         CreationCollisionOption.OpenIfExists);
        }

        private async Task<Dictionary<string, DataTrends>> RetrieveDataTrendsCache()
        {
            var file = await dataTrendsCacheFile.Value;
            await dataTrendsSemaphore.WaitAsync();
            try
            {
                var cache = (Dictionary<string, DataTrends>)await file.ReadJsonFileAs(typeof(Dictionary<string, DataTrends>));
                return cache ?? new Dictionary<String, DataTrends>();
            }
            finally
            {
                dataTrendsSemaphore.Release();
            }
        }
    }
}