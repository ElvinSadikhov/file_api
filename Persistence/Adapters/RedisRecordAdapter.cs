using System.Text.Json;
using Application.Ports;
using Domain;
using RedLockNet;
using RedLockNet.SERedis;
using StackExchange.Redis;

namespace Persistence.Adapters;

public class RedisRecordAdapter(IConnectionMultiplexer redisClient) : IRecordPort
{
    private readonly IDatabase _db = redisClient.GetDatabase();
    private readonly RedLockFactory _redlockFactory = RedLockFactory.Create([redisClient as ConnectionMultiplexer]);
    private static readonly String _keyPrefix = "uploadId-";
    private String GenerateKey(string uploadId) => $"{_keyPrefix}{uploadId}";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public async Task<Record> Create(string uploadId, string remoteUploadId, string objectKey, int partCount,
        long partSizeInBytes,
        Dictionary<string, dynamic> metadata, TimeSpan? expiration)
    {
        var record = new Record
        {
            UploadId = uploadId,
            RemoteUploadId = remoteUploadId,
            ObjectKey = objectKey,
            PartCount = partCount,
            PartSizeInBytes = partSizeInBytes,
            Metadata = metadata,
            // ExpirationTimeSpan = expiration,
            ExpirationDateTimeUtc = expiration is not null ? DateTime.UtcNow.Add((TimeSpan)expiration) : null,
            PartNumbersWithTags = new Dictionary<int, string>()
        };

        var json = JsonSerializer.Serialize(record, JsonOptions);

        //! I think no need to make redis clear it automatically, i need to do it myself.
        // await _db.StringSetAsync(uploadId, json, expiry: expiration);
        await _db.StringSetAsync(GenerateKey(uploadId), json, expiry: null);

        return record;
    }

    public async Task Update(Record recordToBeUpdated)
    {
        if (string.IsNullOrWhiteSpace(recordToBeUpdated.UploadId))
            throw new ArgumentException("UploadId cannot be null or empty.");

        var json = JsonSerializer.Serialize(recordToBeUpdated, JsonOptions);
        await _db.StringSetAsync(GenerateKey(recordToBeUpdated.UploadId), json,
            expiry: recordToBeUpdated.ExpirationDateTimeUtc?.Subtract(DateTime.UtcNow));
    }

    public async Task AddPartNumbersWithTagsEntryByUploadId(string uploadId, KeyValuePair<int, string> entry)
    {
        using (var redLock = await CreateLockAsync($"upload:{GenerateKey(uploadId)}:lock"))
        {
            if (!redLock.IsAcquired)
                throw new Exception(
                    "Could not acquire the distributed lock for AddPartNumbersWithTagsEntryByUploadId operation.");

            var record = await GetByUploadId(uploadId);
            record!.PartNumbersWithTags.Add(entry.Key, entry.Value);
            record.ExpirationDateTimeUtc = DateTime.UtcNow.AddHours(1);
            await Update(record);
        }
    }

    public async Task AddAdditionalMetadataByUploadId(string uploadId, Dictionary<string, dynamic> metadata)
    {
        using (var redLock = await CreateLockAsync(
                   $"update_metadata:{GenerateKey(uploadId)}:lock",
                   expiry: TimeSpan.FromSeconds(10),
                   wait: TimeSpan.FromSeconds(5),
                   retry: TimeSpan.FromMilliseconds(50)
               ))
        {
            if (!redLock.IsAcquired)
                throw new Exception(
                    "Could not acquire the distributed lock for AddAdditionalMetadataByUploadId operation.");

            var record = await GetByUploadId(uploadId);
            if (record is null)
                throw new Exception($"Record with uploadId {uploadId} not found.");

            foreach (var kvp in metadata)
            {
                record.Metadata[kvp.Key] = kvp.Value;
            }

            await Update(record);
        }
    }

    public async Task<List<Record>> GetAll()
    {
        var records = new List<Record>();
        var endpoints = _db.Multiplexer.GetEndPoints();

        foreach (var endpoint in endpoints)
        {
            var server = _db.Multiplexer.GetServer(endpoint);

            // only scan string keys with the specific prefix
            foreach (var key in server.Keys(pattern: $"{_keyPrefix}*"))
            {
                var value = await _db.StringGetAsync(key);
                if (!value.IsNullOrEmpty)
                {
                    try
                    {
                        var record = JsonSerializer.Deserialize<Record>(value!, JsonOptions);
                        if (record != null)
                            records.Add(record);
                    }
                    catch
                    {
                        // skip if error
                    }
                }
            }
        }

        return records;
    }


    public async Task<Record?> GetByUploadId(string uploadId)
    {
        var json = await _db.StringGetAsync(GenerateKey(uploadId));
        var record = json.IsNullOrEmpty
            ? null
            : JsonSerializer.Deserialize<Record>(json!, JsonOptions);

        if (record is null || record.HasExpired()) return null;

        return record;
    }

    public async Task<List<int>> GetLeftChunks(string uploadId)
    {
        var record = (await GetByUploadId(uploadId))!;

        return record.GetLeftParts();
    }

    public async Task DeleteByUploadId(string uploadId)
    {
        await _db.KeyDeleteAsync(GenerateKey(uploadId));
    }

    /// <summary>
    /// Creates redlock for distributed locking.
    /// </summary>
    /// <param name="resource">ID of lock.</param>
    /// <param name="expiry">IDK. Defaults to 30 sec.</param>
    /// <param name="wait">Total time to wait. Defaults to 30s.</param>
    /// <param name="retry">Time to retry again. Defaults to 100ms.</param>
    /// <returns></returns>
    private Task<IRedLock> CreateLockAsync(string resource, TimeSpan? expiry = null, TimeSpan? wait = null,
        TimeSpan? retry = null)
    {
        expiry ??= TimeSpan.FromSeconds(30);
        wait ??= TimeSpan.FromSeconds(30);
        retry ??= TimeSpan.FromMilliseconds(100);

        return _redlockFactory.CreateLockAsync(resource, (TimeSpan)expiry, (TimeSpan)wait, (TimeSpan)retry);
    }
}