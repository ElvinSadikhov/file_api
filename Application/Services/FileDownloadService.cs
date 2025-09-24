using Application.Ports;
using Application.Services.Abstractions;

namespace Application.Services;

public class FileDownloadService(IFilePort filePort) : IFileDownloadService
{
    public Task<string> GenerateDownloadUrl(string objectKey, string? ownerId = null)
    {
        return filePort.GenerateDownloadPreSignedUrl(objectKey, DateTime.Now.AddMinutes(15), ownerId);
    }

    public Task<(Stream, long)> DownloadPart(string objectKey, int partNumber, long partSizeInBytes, long totalSizeInBytes, string? ownerId = null)
    {
        return filePort.DownloadPart(objectKey, partNumber, partSizeInBytes, totalSizeInBytes, ownerId);
    }
}