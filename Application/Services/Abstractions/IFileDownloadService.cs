using AttributeInjection.Attributes.ForAbstracts;

namespace Application.Services.Abstractions;

[Scoped]
public interface IFileDownloadService
{
    Task<string> GenerateDownloadUrl(string objectKey);

    /// <summary>
    /// Downloads a part of file from storage.
    /// Calculates the range of bytes to be downloaded based on the part number, part size and total size.
    /// </summary>
    /// <param name="objectKey">Directory and file name combined as path.</param>
    /// <param name="partNumber">1-based number of a part.</param>
    /// <param name="partSizeInBytes">Size of individual parts in file in bytes.</param>
    /// <param name="totalSizeInBytes">Total size of file in bytes.</param>
    /// <returns>
    /// Stream of the part and size of the part in bytes.
    /// </returns>
    Task<(Stream, long)> DownloadPart(
        string objectKey,
        int partNumber,
        long partSizeInBytes,
        long totalSizeInBytes
    );
}