using AttributeInjection.Attributes.ForAbstracts;

namespace Application.Services.Abstractions;

[Scoped]
public interface IFileUploadService
{
    /// <summary>
    /// Method for starting a new file upload session.
    /// </summary>
    /// <param name="objectKey">Directory and full file name combined as a path.</param>
    /// <param name="partCount">Number of total chunks that will be sent.</param>
    /// <param name="partSizeInBytes">Size of each chunk in megabytes.</param>
    /// <param name="metadata">Additional data for business logic of client.</param>
    /// <returns>
    /// A unique upload ID (string) that can be used to reference this upload session.
    /// </returns>
    Task<string> InitMultipartUpload(
        string objectKey,
        int partCount,
        long partSizeInBytes,
        Dictionary<string, dynamic> metadata
    );

    /// <summary>
    /// Method for uploading a single chunk of a file.
    /// </summary>
    /// <param name="uploadId">ID to upload chunks for.</param>
    /// <param name="partNumber">The number of the chunk that being uploaded.</param>
    /// <param name="inputStream">Stream of data to be uploaded.</param>
    /// <returns>Indexes of left chunks.</returns>
    Task<List<int>> UploadPart(string uploadId, int partNumber, Stream inputStream);

    Task<List<int>> GetLeftChunks(string uploadId);

    /// <summary>
    /// Completes upload by merging and preparing the whole file.
    /// </summary>
    /// <returns>Metadata set at init step.</returns>
    Task<Dictionary<string, dynamic>> CompleteUpload(string uploadId);
    
    Task AbortUpload(string uploadId);

    Task UploadFullFile(string objectKey, Stream inputStream);
}