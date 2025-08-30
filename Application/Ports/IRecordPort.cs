using AttributeInjection.Attributes.ForAbstracts;
using Domain;

namespace Application.Ports;

[Scoped]
public interface IRecordPort
{
    Task<Record> Create(
        string uploadId,
        string remoteUploadId,
        int chunkCount,
        double chunkSizeInMb,
        Dictionary<string, dynamic> metadata,
        DateTime expirationDate,
        List<int> leftChunks
    );
    
    Task Update(Record recordToBeUpdated);
    
    Task<Record> GetByUploadId(string uploadId);

    Task<List<int>> GetLeftChunks(string uploadId);
    
    
    Task Delete(string uploadId);
}