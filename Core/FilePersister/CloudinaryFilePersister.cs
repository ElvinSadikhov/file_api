using AttributeInjection.Attributes.ForConretes;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Configuration;

namespace Core.FilePersister;

[UseThis]
public class CloudinaryFilePersister : IFilePersister
{
    private readonly Cloudinary _client;

    public CloudinaryFilePersister(IConfiguration configuration)
    {
        Account account = new(
            configuration["Cloudinary:CloudName"]!,
            configuration["Cloudinary:ApiKey"]!,
            configuration["Cloudinary:ApiSecret"]!
        );
        _client = new Cloudinary(account);
    }

    public async Task<string> SaveAsync(string fileName, string base64File, string? directory = null)
    {
        var fileStream = GetStreamFromBase64File(base64File);
        FileDescription fileDescription = new FileDescription(fileName, fileStream);

        AutoUploadParams uploadParams = new AutoUploadParams()
        {
            File = fileDescription,
            DisplayName = fileName,
            Folder = directory
        };

        RawUploadResult result = await _client.UploadAsync(uploadParams);

        return result.PublicId;
    }

    public async Task<string> GetResourceUrlByIdAsync(string id)
    {
        GetResourceResult result = await _client.GetResourceAsync(new GetResourceParams(id));
        return result.SecureUrl;
    }

    private Stream GetStreamFromBase64File(string base64File)
    {
        byte[] fileBytes = Convert.FromBase64String(base64File); 
        return new MemoryStream(fileBytes);
    }
}