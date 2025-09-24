namespace Domain;

public class Record
{
    public string UploadId { get; set; }
    public string RemoteUploadId { get; set; }
    
    public string? ownerId { get; set; }
    public int PartCount { get; set; }
    public long PartSizeInBytes { get; set; }
    public Dictionary<string, dynamic> Metadata { get; set; }

    // public TimeSpan? ExpirationTimeSpan
    // {
    //     get => ExpirationTimeSpan ?? (ExpirationDateTime is not null ? ExpirationDateTime - DateTime.UtcNow : null);
    //     set => ExpirationDateTime = DateTime.UtcNow + value;
    // }

    public DateTime? ExpirationDateTimeUtc { get; set; }

    public string ObjectKey { get; set; }
    public Dictionary<int, string> PartNumbersWithTags { get; set; }

    public List<int> GetLeftParts()
    {
        return Enumerable.Range(1, PartCount)
            .Where(i => !PartNumbersWithTags.ContainsKey(i) ||
                        string.IsNullOrEmpty(PartNumbersWithTags[i]))
            .ToList();
    }

    public bool GetIsCompleted()
    {
        return PartNumbersWithTags.Count == PartCount;
    }

    public bool HasExpired()
    {
        return ExpirationDateTimeUtc is not null && DateTime.UtcNow >= ExpirationDateTimeUtc;
    }

    public bool IsOwnedBy(string? ownerId)
    {
        if (this.ownerId is null) return true;
        if (ownerId is null) return false;
        
        return string.Equals(this.ownerId, ownerId);
    }
}