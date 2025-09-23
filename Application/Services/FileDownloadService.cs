using Application.Ports;
using Application.Services.Abstractions;

namespace Application.Services;

public class FileDownloadService(IFilePort filePort) : IFileDownloadService
{
    public Task<string> GenerateDownloadUrl(string objectKey)
    {
        return filePort.GenerateDownloadPreSignedUrl(objectKey, DateTime.Now.AddMinutes(15));
    }

    public Task<(Stream, long)> DownloadPart(string objectKey, int partNumber, long partSizeInBytes, long totalSizeInBytes)
    {
        return filePort.DownloadPart(objectKey, partNumber, partSizeInBytes, totalSizeInBytes);
    }
}