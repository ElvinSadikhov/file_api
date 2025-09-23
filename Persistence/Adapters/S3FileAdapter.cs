using Amazon.S3;
using Amazon.S3.Model;
using Application.Ports;
using AtomCore.ExceptionHandling.Exceptions;

namespace Persistence.Adapters;

public class S3FileAdapter : IFilePort
{
    private static AmazonS3Client? _client;
    private static string? _bucketName;

    public S3FileAdapter()
    {
        GetClient();
    }

    private AmazonS3Client GetClient()
    {
        if (_client is not null) return _client;

        var awsAccessKeyId = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID")!;
        var awsSecretAccessKey = Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY")!;
        var awsS3RegionSystemName = Environment.GetEnvironmentVariable("AWS_S3_REGION_SYSTEM_NAME");
        _bucketName = Environment.GetEnvironmentVariable("AWS_S3_BUCKET_NAME")!;

        //! these are optional
        var s3ServiceUrl = Environment.GetEnvironmentVariable("S3_SERVICE_URL");
        var s3ForcePathStyle = string.Equals(Environment.GetEnvironmentVariable("S3_FORCE_PATH_STYLE"), "true");

        if (s3ServiceUrl is not null)
        {
            var config = new AmazonS3Config
            {
                ServiceURL = s3ServiceUrl,
                ForcePathStyle = s3ForcePathStyle,
            };

            _client = new AmazonS3Client(awsAccessKeyId, awsSecretAccessKey, config);
        }
        else
        {
            _client = new AmazonS3Client(
                awsAccessKeyId,
                awsSecretAccessKey,
                // Amazon.RegionEndpoint.USEast1
                Amazon.RegionEndpoint.EnumerableAllRegions.First(re =>
                    string.Equals(re.SystemName, awsS3RegionSystemName, StringComparison.OrdinalIgnoreCase))
            );
        }

        return _client;
    }

    public async Task<string> Init(string objectKey)
    {
        var request = new InitiateMultipartUploadRequest()
        {
            BucketName = _bucketName,
            Key = objectKey,
        };
        var response = await GetClient().InitiateMultipartUploadAsync(request);
        return response.UploadId;
    }

    public async Task<string> UploadPart(string uploadId, string objectKey, int partNumber, Stream inputStream,
        long? partSize)
    {
        var request = new UploadPartRequest()
        {
            BucketName = _bucketName,
            Key = objectKey,
            UploadId = uploadId,
            PartNumber = partNumber,
            PartSize = partSize,
            InputStream = inputStream,
        };

        var response = await GetClient().UploadPartAsync(request);

        bool isSuccessful = (int)response.HttpStatusCode >= 200 && (int)response.HttpStatusCode < 300;
        if (!isSuccessful)
            throw new InfrastructureException(
                $"Error uploading part object: {objectKey} with uploadId: {uploadId} -> {response.HttpStatusCode}, {response.ResponseMetadata}");

        return response.ETag;
    }

    public async Task CompleteUpload(string uploadId, string objectKey, Dictionary<int, string> partNumbersWithTags)
    {
        var request = new CompleteMultipartUploadRequest
        {
            BucketName = _bucketName,
            Key = objectKey,
            UploadId = uploadId,
            PartETags = partNumbersWithTags.Select(p => new PartETag(p.Key, p.Value)).ToList(),
        };

        var response = await GetClient().CompleteMultipartUploadAsync(request);

        bool isSuccessful = (int)response.HttpStatusCode >= 200 && (int)response.HttpStatusCode < 300;
        if (!isSuccessful)
            throw new InfrastructureException(
                $"Error completing upload object: {objectKey} with uploadId: {uploadId} -> {response.HttpStatusCode}, {response.ResponseMetadata}");
    }

    public async Task AbortUpload(string uploadId, string objectKey)
    {
        var request = new AbortMultipartUploadRequest
        {
            BucketName = _bucketName,
            Key = objectKey,
            UploadId = uploadId,
        };

        var response = await GetClient().AbortMultipartUploadAsync(request);

        bool isSuccessful = (int)response.HttpStatusCode >= 200 && (int)response.HttpStatusCode < 300;
        if (!isSuccessful)
            throw new InfrastructureException(
                $"Error aborting upload object: {objectKey} with uploadId: {uploadId} -> {response.HttpStatusCode}, {response.ResponseMetadata}");
    }

    public async Task<(Stream, long)> DownloadPart(string objectKey, int partNumber, long partSizeInBytes,
        long totalSizeInBytes)
    {
        long start = (partNumber - 1) * partSizeInBytes;
        long end = Math.Min(start + partSizeInBytes, totalSizeInBytes) - 1;

        if (start >= totalSizeInBytes)
            throw new ArgumentOutOfRangeException(nameof(partNumber), "Part start exceeds object size");

        var request = new GetObjectRequest
        {
            BucketName = _bucketName,
            Key = objectKey,
            ByteRange = new ByteRange(start, end)
        };

        var response = await GetClient().GetObjectAsync(request);

        bool isSuccessful = (int)response.HttpStatusCode >= 200 && (int)response.HttpStatusCode < 300;
        if (!isSuccessful)
            throw new InfrastructureException(
                $"Error aborting upload object: {objectKey} with partNumber: {partNumber} -> {response.HttpStatusCode}, {response.ResponseMetadata}");

        return (response.ResponseStream, end - start + 1);
    }

    public async Task<long> GetFileSizeInBytes(string objectKey)
    {
        var request = new GetObjectAttributesRequest()
        {
            BucketName = _bucketName,
            Key = objectKey,
            ObjectAttributes = new List<ObjectAttributes> { ObjectAttributes.ObjectSize }
        };
        var response = await GetClient().GetObjectAttributesAsync(request);

        return (long)response.ObjectSize!;
    }

    public async Task<string> GenerateDownloadPreSignedUrl(string objectKey, DateTime expiresOn)
    {
        var request = new GetPreSignedUrlRequest()
        {
            BucketName = _bucketName,
            Key = objectKey,
            Expires = expiresOn,
            Verb = HttpVerb.GET
        };

        return await GetClient().GetPreSignedURLAsync(request);
    }

    public async Task<List<string>> DeleteMultiple(List<string> objectKeys)
    {
        var request = new DeleteObjectsRequest
        {
            BucketName = _bucketName!,
            Objects = objectKeys.Select(key => new KeyVersion { Key = key }).ToList(),
            Quiet = false,
        };
        var response = await GetClient().DeleteObjectsAsync(request);

        return response.DeletedObjects.Select(o => o.Key).ToList();
    }

    public async Task UploadFullFile(string objectKey, Stream inputStream)
    {
        var request = new PutObjectRequest()
        {
            BucketName = _bucketName,
            Key = objectKey,
            InputStream = inputStream,
            // StreamTransferProgress = 
        };
        var response = await GetClient().PutObjectAsync(request);
        
        bool isSuccessful = (int)response.HttpStatusCode >= 200 && (int)response.HttpStatusCode < 300;
        if (!isSuccessful)
            throw new InfrastructureException(
                $"Error aborting upload object: {objectKey} -> {response.HttpStatusCode}, {response.ResponseMetadata}");
    }
}