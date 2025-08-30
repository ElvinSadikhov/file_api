using Microsoft.AspNetCore.Http;

namespace Core.Extensions;

public static class FormFileExtensions
{
    public static async Task<string> ToBase64String(this IFormFile file)
    {
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        byte[] fileBytes = memoryStream.ToArray();
        return Convert.ToBase64String(fileBytes);
    }
    
    public static float LengthInMb(this IFormFile file)
    {
        return (float) file.Length / 1024 / 1024;
    }
}