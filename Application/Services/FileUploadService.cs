using System.Security.Cryptography;
using Application.Ports;
using Application.Services.Abstractions;
using AtomCore.ExceptionHandling.Exceptions;

namespace Application.Services;

public class FileUploadService(IRecordPort recordPort, IFilePort filePort) : IFileUploadService
{
    public async Task<string> Init(int chunkCount, double chunkSizeInMb, Dictionary<string, dynamic> metadata)
    {
        // init in filePort and get remoteUploadId
        var remoteUploadId = await filePort.Init(chunkCount, chunkSizeInMb);

        // generate GUID for uploadId
        var uploadId = Guid.NewGuid().ToString();

        // add chunkCount, chunkSizeInMb, metadata
        // set expirationDate(or `expire` if redis) to 1h (for ex.)
        // set a list of leftChunks as [0, 1, 2, ..., chunkCount-1]
        await recordPort.Create(
            uploadId,
            remoteUploadId,
            chunkCount,
            chunkSizeInMb,
            metadata,
            expirationDate: DateTime.UtcNow.AddHours(1),
            leftChunks: Enumerable.Range(0, chunkCount).ToList()
        );

        return uploadId;
    }

    public async Task<List<int>> UploadChunk(string uploadId, int chunkIndex, byte[] chunkData)
    {
        // get remoteUploadId from db by uploadId
        var record = await recordPort.GetByUploadId(uploadId);
        string remoteUploadId = record.RemoteUploadId;

        // upload chunk to filePort
        await filePort.UploadChunk(remoteUploadId, chunkIndex, chunkData);

        // mark chunk as uploaded in db (remove chunkIndex from leftChunks)
        record.LeftChunks.RemoveAll(r => r == chunkIndex);
        await recordPort.Update(record);

        return record.LeftChunks;
    }

    public async Task<List<int>> GetLeftChunks(string uploadId)
    {
        // get leftChunks from db by uploadId
        var record = await recordPort.GetByUploadId(uploadId);
        
        return record.LeftChunks;
    }

    public async Task CompleteUpload(string uploadId, string clintChecksum)
    {
        var record = await recordPort.GetByUploadId(uploadId);
        
        // check if all chunks are uploaded (leftChunks is empty)
        if (record.LeftChunks.Count > 0)
            throw new BusinessException("There are still chunks left to upload.");

        // check checksum of the merged file and the provided checksum
        byte[] mergedChunks = await filePort.MergeChunks(record.RemoteUploadId);
        using var sha256 = SHA256.Create();
        byte[] checksumHash = sha256.ComputeHash(mergedChunks);
        string checksum = Convert.ToHexString(checksumHash).ToLowerInvariant();
        //? maybe we don't need to do .ToLowerInvariant() for clientChecksum
        if (!checksum.Equals(clintChecksum, StringComparison.InvariantCultureIgnoreCase))
        {
            // todo: mark record as corrupted
            // todo: delete corrupted record later
            
            throw new BusinessException("File integrity check failed.");
        }
    }
}