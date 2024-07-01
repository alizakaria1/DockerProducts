using System.Text.Json;

namespace DALC.IRepositories
{
    public interface ICacheRepository
    {
        public Task<T> GetOrAddCachedObjectAsync<T>(string key, Func<Task<T>> getItem);

        public Task<T> GetOrAddCachedObjectAsync<T>(string key, string allRecordsKey, Func<Task<T>> getItem);

        public Task<bool> InsertItemAndSyncRedisAsync<T>(T newItem, string itemKey, string allRecordsKey);

        public Task<bool> DeleteItemAndSyncRedisAsync<T>(string itemKey, string allRecordsKey, string itemId);

        public Task<bool> UpdateItemAndSyncRedisAsync<T>(string itemKey, T updatedItem, string allRecordsKey, string itemId);

        public Task<bool> DeleteUploadedFile<T>(string fileId, string itemKey, string allRecordsKey, string itemId);

    }
}
