namespace Core.Cache;

public class CachedEntity<T>
{
    public required T Entity { get; set; }
    private DateTime ExpirationDate { get; set; }

    public static CachedEntity<T> Create(T entity, TimeSpan cacheSpan)
    {
        return new CachedEntity<T>
        {
            Entity = entity,
            ExpirationDate = DateTime.UtcNow + cacheSpan
        };
    }

    public bool HasExpired()
    {
        return ExpirationDate.CompareTo(DateTime.UtcNow) < 0;
    }
}