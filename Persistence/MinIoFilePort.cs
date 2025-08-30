using Application.Ports;

namespace Persistence;

public class MinIoFilePort : IFilePort
{
    public Task<string> Init(int chunkCount, double chunkSizeInMb)
    {
        throw new NotImplementedException();
    }

    public Task UploadChunk(string uploadId, int chunkIndex, byte[] chunkData)
    {
        throw new NotImplementedException();
    }

    public Task<byte[]> MergeChunks(string uploadId)
    {
        throw new NotImplementedException();
    }
}