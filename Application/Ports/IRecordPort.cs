using AttributeInjection.Attributes.ForAbstracts;
using Domain;

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
        TimeSpan? expiration
    );

    Task Update(Record recordToBeUpdated);
    
    Task AddPartNumbersWithTagsEntryByUploadId(string uploadId, KeyValuePair<int, string> entry);

    Task<List<Record>> GetAll();
    
    Task<Record?> GetByUploadId(string uploadId);

    Task<List<int>> GetLeftChunks(string uploadId);

    Task DeleteByUploadId(string uploadId);
}