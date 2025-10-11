using AttributeInjection.Attributes.ForAbstracts;
using Domain;
using RedLockNet;

namespace Application.Ports;

[Singelton]
public interface IRecordPort
{
    Task<Record> Create(string uploadId,
        string remoteUploadId,
        string objectKey,
        int partCount,
        long partSizeInBytes,
        Dictionary<string, dynamic> metadata,
        TimeSpan? expiration,
        string? ownerId = null
    );

    Task Update(Record recordToBeUpdated, IRedLock? externalLock = null);
    
    Task AddPartNumbersWithTagsEntryByUploadId(string uploadId, KeyValuePair<int, string> entry);
    
    Task AddAdditionalMetadataByUploadId(string uploadId, Dictionary<string, dynamic> metadata, string? ownerId = null);

    Task<List<Record>> GetAll();
    
    Task<Record?> GetByUploadId(string uploadId);

    Task DeleteByUploadId(string uploadId);
}