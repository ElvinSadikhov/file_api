namespace Application.Dtos;

public class InitUploadRequestDto
{
    public int ChunkCount { get; set; }
    public double ChunkSizeInMb { get; set; }
    public Dictionary<string, dynamic> Metadata { get; set; }
}