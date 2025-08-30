using AttributeInjection.Attributes.ForAbstracts;

namespace Core.FilePersister;

[Scoped]
public interface IFilePersister
{
    Task<string> SaveAsync(
        string fileName,
        string base64File,
        string? directory = null
    );

    Task<string> GetResourceUrlByIdAsync(string id);
}