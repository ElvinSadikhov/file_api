using AttributeInjection.Attributes.ForAbstracts;

namespace Application.Ports;

[Singelton]
public interface IFilePort
{
    /// <summary>
    /// Initializes multipart upload procedure.
    /// </summary>
    /// <param name="objectKey">Directory and file name combined as path.</param>
    /// <returns>Upload ID of new object.</returns>
    Task<string> Init(
        string objectKey
    );

    /// <summary></summary>
    /// <param name="uploadId">ID got from init method.</param>
    /// <param name="objectKey">Directory and file name combined as path.</param>
    /// <param name="partNumber">1- based part number.</param>
    /// <param name="inputStream">Stream of data to be uploaded.</param>
    /// <param name="partSize">Part size in bytes.</param>
    /// <returns></returns>
    Task<string> UploadPart(
        string uploadId,
        string objectKey,
        int partNumber,
        Stream inputStream,
        long? partSize
    );

    Task CompleteUpload(
        string uploadId,
        string objectKey,
        Dictionary<int, string> partNumbersWithTags
    );

    Task AbortUpload(
        string uploadId,
        string objectKey
    );

    Task<(Stream, long)> DownloadPart(
        string objectKey,
        int partNumber,
        long partSizeInBytes,
        long totalSizeInBytes
    );

    Task<long> GetFileSizeInBytes(
        string objectKey
    );

    Task<string> GenerateDownloadPreSignedUrl(
        string objectKey,
        DateTime expiresOn
    );

    Task<List<string>> DeleteMultiple(
        List<string> objectKeys
    );
    
    /// <summary>
    /// Should be used only for small files, that can be uploaded in a single request.
    /// </summary>
    Task UploadFullFile(
        string objectKey,
        Stream inputStream
    );
}