using Application.Ports;
using Application.Services.Abstractions;

namespace Application.Services;

public class FileUtilsService(IFilePort filePort) : IFileUtilsService
{
    public Task<long> GetFileSizeInBytes(string objectKey)
    {
        return filePort.GetFileSizeInBytes(objectKey);
    }

    public Task<List<string>> DeleteFiles(List<string> objectKeys)
    {
        return filePort.DeleteMultiple(objectKeys);
    }
}