using DALC.IRepositories;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Shared.Constants;
using Shared.Models;
using StackExchange.Redis;
using System.Reflection;
using System.Text.Json;

namespace DALC.Repositories
{
    public class CacheRepository : ICacheRepository
    {
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _database;
        //private bool isRedisEnabled;

        public CacheRepository()
        {
            _redis = ConnectionMultiplexer.Connect(ConfigVariables.redisConnectionString);
            _database = _redis.GetDatabase();
        }

        public async Task<T> GetOrAddCachedObjectAsync<T>(string key, Func<Task<T>> getItem)
        {

            try
            {
                if (!ConfigVariables.isRedisEnabled)
                {
                    return await getItem();
                }

                var obj = await _database.StringGetAsync(key);

                if (obj.HasValue)
                {
                    T jsonObject = JsonConvert.DeserializeObject<T>(obj);

                    return jsonObject;
                }
                else
                {
                    var item = await getItem();

                    if (item == null)
                    {
                        return default(T);
                    }

                    var settings = new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    };

                    string jsonString = JsonConvert.SerializeObject(item,settings);

                    var res = await _database.StringSetAsync(key, jsonString);

                    return item;
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<T> GetOrAddCachedObjectAsync<T>(string key, string allRecordsKey, Func<Task<T>> getItem)
        {

            if (!ConfigVariables.isRedisEnabled)
            {
                return await getItem();
            }

            // Check if the item exists in the cache
            var obj = await _database.StringGetAsync(key);

            if (obj.HasValue)
            {
                // Deserialize the item from the cache
                T cachedObject = JsonConvert.DeserializeObject<T>(obj);
                return cachedObject;
            }
            else
            {
                // Get the item using the provided function
                var item = await getItem();

                if (item == null)
                {
                    return default(T);
                }

                // Serialize the item to JSON
                string jsonString = JsonConvert.SerializeObject(item);

                // Add the item to the cache
                await _database.StringSetAsync(key, jsonString);

                // Check if the list of all products contains the product
                var allRecordsJson = await _database.StringGetAsync(allRecordsKey);
                List<T> allRecords;
                if (allRecordsJson.HasValue)
                {
                    allRecords = JsonConvert.DeserializeObject<List<T>>(allRecordsJson);
                }
                else
                {
                    allRecords = new List<T>();
                }

                // Extract the Id property from the item
                var itemId = GetIdProperty(item)?.ToString();
                if (itemId == null)
                {
                    throw new InvalidOperationException("The item does not have a valid Id property.");
                }

                // Check if the item already exists in the list
                var existingItem = allRecords.SingleOrDefault(record =>
                {
                    var recordId = GetIdProperty(record)?.ToString();
                    return recordId == itemId;
                });

                if (existingItem == null)
                {
                    // Add the item to the list if it does not already exist
                    allRecords.Add(item);

                    // Serialize the updated list back to JSON
                    var updatedRecordsJson = JsonConvert.SerializeObject(allRecords);

                    // Update the list in Redis
                    await _database.StringSetAsync(allRecordsKey, updatedRecordsJson);
                }

                return item;
            }
        }

        public async Task<bool> DeleteItemAndSyncRedisAsync<T>(string itemKey, string allRecordsKey, string itemId)
        {

            if (!ConfigVariables.isRedisEnabled)
            {
                return false;
            }

            // Create a Redis transaction
            var transaction = _database.CreateTransaction();

            // Step 2: Delete the item from Redis
            var deleteItemTask = transaction.KeyDeleteAsync(itemKey);

            // Step 3: Get the current list of all records from Redis
            var allRecordsJson = await _database.StringGetAsync(allRecordsKey);
            if (!allRecordsJson.HasValue)
            {
                // If the list does not exist, return false or handle the error
                return false;
            }

            // Deserialize the JSON list to a C# list
            var allRecords = JsonConvert.DeserializeObject<List<T>>(allRecordsJson);

            // Step 4: Find and remove the deleted item from the list
            // Assuming T has a property "Key"
            //var itemToRemove = allRecords.FirstOrDefault(item => item.GetType().GetProperty("Id")?.GetValue(item).ToString() == itemId);
            var itemToRemove = allRecords.FirstOrDefault(item =>
            {
                var idProperty = item.GetType().GetProperty("Id");
                var fileIdProperty = item.GetType().GetProperty("FileId");
                var idValue = idProperty?.GetValue(item)?.ToString();
                var fileIdValue = fileIdProperty?.GetValue(item)?.ToString();
                return idValue == itemId || fileIdValue == itemId;
            });
            if (itemToRemove != null)
            {
                allRecords.Remove(itemToRemove);
            }

            // Serialize the updated list back to JSON
            var updatedRecordsJson = JsonConvert.SerializeObject(allRecords);

            // Step 5: Update the list in Redis
            var updateListTask = transaction.StringSetAsync(allRecordsKey, updatedRecordsJson);

            // Execute the transaction
            bool transactionSuccess = await transaction.ExecuteAsync();

            // If the transaction was successful, return true
            return transactionSuccess;
        }

        public async Task<bool> UpdateItemAndSyncRedisAsync<T>(string itemKey, T updatedItem, string allRecordsKey, string itemId)
        {

            if (!ConfigVariables.isRedisEnabled)
            {
                return false;
            }

            // Create a Redis transaction
            var transaction = _database.CreateTransaction();

            // Step 2: Update the specific item in Redis
            var updateItemTask = transaction.StringSetAsync(itemKey, JsonConvert.SerializeObject(updatedItem));

            // Step 3: Get the current list of all records from Redis
            var allRecordsJson = await _database.StringGetAsync(allRecordsKey);
            if (!allRecordsJson.HasValue)
            {
                // If the list does not exist, return false or handle the error
                return false;
            }

            // Deserialize the JSON list to a C# list
            var allRecords = JsonConvert.DeserializeObject<List<T>>(allRecordsJson);

            // Step 4: Find and update the item in the list
            // Assuming T has a property "Id"
            var itemToUpdate = allRecords.SingleOrDefault(item => item.GetType().GetProperty("Id")?.GetValue(item).ToString() == itemId);
            if (itemToUpdate != null)
            {
                int index = allRecords.IndexOf(itemToUpdate);
                allRecords[index] = updatedItem;
            }

            // Serialize the updated list back to JSON
            var updatedRecordsJson = JsonConvert.SerializeObject(allRecords);

            // Step 5: Update the list in Redis
            var updateListTask = transaction.StringSetAsync(allRecordsKey, updatedRecordsJson);

            // Execute the transaction
            bool transactionSuccess = await transaction.ExecuteAsync();

            // If the transaction was successful, return true
            return transactionSuccess;
        }

        public async Task<bool> InsertItemAndSyncRedisAsync<T>(T newItem, string itemKey, string allRecordsKey)
        {

            if (!ConfigVariables.isRedisEnabled)
            {
                return false;
            }

            // Create a Redis transaction
            var transaction = _database.CreateTransaction();

            // Step 2: Insert the new item into Redis
            var insertItemTask = transaction.StringSetAsync(itemKey, JsonConvert.SerializeObject(newItem));

            // Step 3: Get the current list of all records from Redis
            var allRecordsJson = await _database.StringGetAsync(allRecordsKey);
            List<T> allRecords;

            if (allRecordsJson.HasValue)
            {
                // Deserialize the JSON list to a C# list
                allRecords = JsonConvert.DeserializeObject<List<T>>(allRecordsJson);
            }
            else
            {
                // If the list does not exist, initialize a new list
                allRecords = new List<T>();
            }

            // Step 4: Add the new item to the list
            allRecords.Add(newItem);

            // Serialize the updated list back to JSON
            var updatedRecordsJson = JsonConvert.SerializeObject(allRecords);

            // Step 5: Update the list in Redis
            var updateListTask = transaction.StringSetAsync(allRecordsKey, updatedRecordsJson);

            // Execute the transaction
            bool transactionSuccess = await transaction.ExecuteAsync();

            // If the transaction was successful, return true
            return transactionSuccess;
        }

        private object GetIdProperty<T>(T item)
        {
            // Find the property named "Id", "ID", or "Key"
            PropertyInfo idProperty = typeof(T).GetProperty("Id") ?? typeof(T).GetProperty("ID") ?? typeof(T).GetProperty("Key");
            if (idProperty == null)
            {
                throw new InvalidOperationException("The type T does not have a property named 'Id', 'ID', or 'Key'.");
            }

            // Get the value of the property
            return idProperty.GetValue(item);
        }

    }
}
