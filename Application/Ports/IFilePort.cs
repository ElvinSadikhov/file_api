using AttributeInjection.Attributes.ForAbstracts;

namespace Application.Ports;

[Scoped]
public interface IFilePort
{
    Task<string> Init(
        int chunkCount,
        double chunkSizeInMb
    );

    Task UploadChunk(
        string uploadId,
        int chunkIndex,
        byte[] chunkData
    );
    
    Task<byte[]> MergeChunks(string uploadId);
}