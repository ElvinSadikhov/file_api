using AttributeInjection.Attributes.ForAbstracts;

namespace Application.Services.Abstractions;

[Scoped]
public interface IFileUtilsService
{
    Task<long> GetFileSizeInBytes(string objectKey, string? ownerId = null);

    Task UpdateFileMetadata(string uploadId, Dictionary<string, dynamic> metadata);

    Task<List<string>> DeleteFiles(List<string> objectKeys, string? ownerId = null);
}