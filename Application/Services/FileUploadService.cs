using Application.Ports;
using Application.Services.Abstractions;
using AtomCore.ExceptionHandling.Exceptions;
using Domain;

namespace Application.Services;

public class FileUploadService(IRecordPort recordPort, IFilePort filePort) : IFileUploadService
{
    public async Task<string> InitMultipartUpload(string objectKey, int partCount, long partSizeInBytes,
        Dictionary<string, dynamic>? metadata = null, string? ownerId = null)
    {
        // check for maxAllowedPartSize
        var maxAllowedPartSizeInBytes =
            int.Parse(Environment.GetEnvironmentVariable("MAX_ALLOWED_PART_SIZE_IN_BYTES")!);
        if (partSizeInBytes > maxAllowedPartSizeInBytes)
            throw new BusinessException(
                $"Part size exceeds the maximum allowed limit of {maxAllowedPartSizeInBytes} bytes.");

        // init in filePort and get remoteUploadId
        var remoteUploadId = await filePort.Init(objectKey, ownerId);

        // generate GUID for uploadId
        var uploadId = Guid.NewGuid().ToString();

        // add partCount, partSizeInBytes, metadata
        // set expirationDate(or `expire` if redis) to 1h (for ex.)
        // set a list of leftChunks as [1, 2, ..., partCount]
        await recordPort.Create(
            uploadId,
            remoteUploadId,
            objectKey,
            partCount,
            partSizeInBytes,
            metadata ?? new Dictionary<string, dynamic>(),
            expiration: TimeSpan.FromHours(1),
            ownerId
        );

        return uploadId;
    }

    public async Task<List<int>> UploadPart(string uploadId, int partNumber, Stream inputStream, string? ownerId = null)
    {
        if (partNumber < 1)
            throw new BusinessException("Part number is 1-based, should be bigger than 0.");

        // get remoteUploadId from db by uploadId
        var record = await recordPort.GetByUploadId(uploadId);
        if (record == null)
            throw new BusinessException("record not found.");
        if (!record.IsOwnedBy(ownerId))
            throw new BusinessException("You are not the owner of this record.");

        // upload chunk to filePort
        bool isLastPart = partNumber == record.PartCount;
        string tag = await filePort.UploadPart(
            record.RemoteUploadId,
            record.ObjectKey,
            partNumber,
            inputStream,
            isLastPart
                ? null
                : record.PartSizeInBytes
        );

        // mark chunk as uploaded in db (remove partNumber from leftChunks) and save tag
        await recordPort.AddPartNumbersWithTagsEntryByUploadId(record.UploadId,
            new KeyValuePair<int, string>(partNumber, tag));

        return record.GetLeftParts();
    }

    public async Task<List<int>> GetLeftParts(string uploadId, string? ownerId = null)
    {
        // get leftChunks from db by uploadId
        var record = await recordPort.GetByUploadId(uploadId);
        
        if (!record!.IsOwnedBy(ownerId))
            throw new BusinessException("You are not the owner of this record.");

        return record.GetLeftParts();
    }

    public async Task<Dictionary<string, dynamic>> CompleteUpload(string uploadId, string? ownerId = null)
    {
        var record = await recordPort.GetByUploadId(uploadId);
        if (record == null)
            throw new BusinessException("record not found.");
        if (!record.IsOwnedBy(ownerId))
            throw new BusinessException("You are not the owner of this record.");

        // check if all chunks are uploaded (leftChunks is empty)
        if (record.GetLeftParts().Count > 0)
            throw new BusinessException("There are still parts left to be uploaded.");

        await filePort.CompleteUpload(record.RemoteUploadId, record.ObjectKey, record.PartNumbersWithTags);

        await recordPort.DeleteByUploadId(uploadId);

        return new Dictionary<string, dynamic>()
        {
            { "objectKey", record.ObjectKey },
            { "metadata", record.Metadata },
        };
    }

    public async Task AbortUpload(string uploadId, string? ownerId = null)
    {
        var record = await recordPort.GetByUploadId(uploadId);
        if (record == null)
            throw new BusinessException("record not found.");
        if (!record.IsOwnedBy(ownerId))
            throw new BusinessException("You are not the owner of this record.");

        await filePort.AbortUpload(record.RemoteUploadId, record.ObjectKey);

        await recordPort.DeleteByUploadId(uploadId);
    }

    public Task UploadFullFile(string objectKey, Stream inputStream, string? ownerId = null)
    {
        return filePort.UploadFullFile(objectKey, inputStream, ownerId);
    }
}