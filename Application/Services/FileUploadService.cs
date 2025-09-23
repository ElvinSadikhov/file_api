using Application.Ports;
using Application.Services.Abstractions;
using AtomCore.ExceptionHandling.Exceptions;

namespace Application.Services;

public class FileUploadService(IRecordPort recordPort, IFilePort filePort) : IFileUploadService
{

    public async Task<string> InitMultipartUpload(string objectKey, int partCount, long partSizeInBytes,
        Dictionary<string, dynamic> metadata)
    {
        // check for maxAllowedPartSize
        var maxAllowedPartSizeInBytes = int.Parse(Environment.GetEnvironmentVariable("MAX_ALLOWED_PART_SIZE_IN_BYTES")!);
        if (partSizeInBytes > maxAllowedPartSizeInBytes) 
            throw new BusinessException($"Part size exceeds the maximum allowed limit of {maxAllowedPartSizeInBytes} bytes.");
        
        // init in filePort and get remoteUploadId
        var remoteUploadId = await filePort.Init(objectKey);

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
            metadata,
            expiration: TimeSpan.FromHours(1)
        );

        return uploadId;
    }
    
    public async Task<List<int>> UploadPart(string uploadId, int partNumber, Stream inputStream)
    {
        if (partNumber < 1)
            throw new BusinessException("Part number is 1-based, should be bigger than 0.");

        // get remoteUploadId from db by uploadId
        var record = await recordPort.GetByUploadId(uploadId);
        if (record == null)
            throw new BusinessException("Upload session not found.");

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
        await recordPort.AddPartNumbersWithTagsEntryByUploadId(record.UploadId, new KeyValuePair<int, string>(partNumber, tag));

        return record.GetLeftParts();
    }

    public async Task<List<int>> GetLeftChunks(string uploadId)
    {
        // get leftChunks from db by uploadId
        var leftChunks = await recordPort.GetLeftChunks(uploadId);

        return leftChunks;
    }

    public async Task<Dictionary<string, dynamic>> CompleteUpload(string uploadId)
    {
        var record = await recordPort.GetByUploadId(uploadId);
        if (record == null)
            throw new BusinessException("Upload session not found.");

        // check if all chunks are uploaded (leftChunks is empty)
        if (record.GetLeftParts().Count > 0)
            throw new BusinessException("There are still parts left to be uploaded.");

        await filePort.CompleteUpload(record.RemoteUploadId, record.ObjectKey, record.PartNumbersWithTags);
        
        await recordPort.DeleteByUploadId(uploadId);

        return record.Metadata;
    }

    public async Task AbortUpload(string uploadId)
    {
        var record = await recordPort.GetByUploadId(uploadId);
        if (record == null)
            throw new BusinessException("Upload session not found.");

        await recordPort.DeleteByUploadId(uploadId);

        await filePort.AbortUpload(record.RemoteUploadId, record.ObjectKey);
    }

    public Task UploadFullFile(string objectKey, Stream inputStream)
    {
        return filePort.UploadFullFile(objectKey, inputStream);
    }
}