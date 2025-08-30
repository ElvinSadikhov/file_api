namespace Domain;

public class Record
{
    public int Id { get; set; }
    public string UploadId { get; set; }
    public string RemoteUploadId { get; set; }
    public int ChunkCount { get; set; }
    public double ChunkSizeInMb { get; set; }
    public Dictionary<string, dynamic> Metadata { get; set; }
    public DateTime ExpirationDate { get; set; }
    public List<int> LeftChunks { get; set; }
}