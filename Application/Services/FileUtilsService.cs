using Application.Ports;
using Application.Services.Abstractions;

namespace Application.Services;

public class FileUtilsService(IFilePort filePort, IRecordPort recordPort) : IFileUtilsService
{
    public Task<long> GetFileSizeInBytes(string objectKey, string? ownerId = null)
    {
        return filePort.GetFileSizeInBytes(objectKey, ownerId);
    }

    public Task UpdateFileMetadata(string uploadId, Dictionary<string, dynamic> metadata)
    {
        return recordPort.AddAdditionalMetadataByUploadId(uploadId, metadata);
    }

    public Task<List<string>> DeleteFiles(List<string> objectKeys, string? ownerId = null)
    {
        return filePort.DeleteMultiple(objectKeys, ownerId);
    }
}