using Application.Ports;
using Application.Services.Abstractions;

namespace Application.Services;

public class FileUtilsService(IFilePort filePort, IRecordPort recordPort) : IFileUtilsService
{
    public Task<long> GetFileSizeInBytes(string objectKey)
    {
        return filePort.GetFileSizeInBytes(objectKey);
    }

    public Task UpdateFileMetadata(string uploadId, Dictionary<string, dynamic> metadata)
    {
        return recordPort.AddAdditionalMetadataByUploadId(uploadId, metadata);
    }

    public Task<List<string>> DeleteFiles(List<string> objectKeys)
    {
        return filePort.DeleteMultiple(objectKeys);
    }
}