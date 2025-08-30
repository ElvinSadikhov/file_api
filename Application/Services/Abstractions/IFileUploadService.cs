using AttributeInjection.Attributes.ForAbstracts;

namespace Application.Services.Abstractions;

[Scoped]
public interface IFileUploadService
{
    /// <summary>
    /// Method for starting a new file upload session.
    /// </summary>
    /// <param name="chunkCount">Number of total chunks that will be sent.</param>
    /// <param name="chunkSizeInMb">Size of each chunk in megabytes.</param>
    /// <param name="metadata">Additional data for business logic of client.</param>
    /// <returns>
    /// A unique upload ID (string) that can be used to reference this upload session.
    /// </returns>
    Task<string> Init(
        int chunkCount,
        double chunkSizeInMb,
        Dictionary<string, dynamic> metadata
    );

    /// <summary>
    /// Method for uploading a single chunk of a file.
    /// </summary>
    /// <param name="uploadId">ID to upload chunks for.</param>
    /// <param name="chunkIndex">The number of the chunk that being uploaded.</param>
    /// <param name="chunkData">Chunk to be uploaded.</param>
    /// <returns>Indexes of left chunks.</returns>
    Task<List<int>> UploadChunk(string uploadId, int chunkIndex, byte[] chunkData);

    Task<List<int>> GetLeftChunks(string uploadId);

    Task CompleteUpload(string uploadId, string clintChecksum);
}