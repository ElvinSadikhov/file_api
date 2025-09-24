namespace Application.Dtos;

public class InitUploadRequestDto
{
    public string ObjectKey { get; set; }
    public int PartCount { get; set; }
    public long PartSizeInBytes { get; set; }
    public Dictionary<string, dynamic>? Metadata { get; set; }
}