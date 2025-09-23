using AttributeInjection.Attributes.ForAbstracts;

namespace Application.Services.Abstractions;

[Scoped]
public interface IFileUtilsService
{
    Task<long> GetFileSizeInBytes(string objectKey);

    Task<List<string>> DeleteFiles(List<string> objectKeys);
}