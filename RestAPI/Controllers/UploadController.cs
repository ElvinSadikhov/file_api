using Application.Dtos;
using Application.Services.Abstractions;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IO;
using RestAPI.Controllers.Commons;

namespace RestAPI.Controllers;

public class UploadController : BaseController
{
    private readonly RecyclableMemoryStreamManager _streamManager;
    private readonly IFileUploadService _fileUploadService;
    private readonly ILogger<UploadController> _logger;

    public UploadController(IFileUploadService fileUploadService, ILogger<UploadController> logger)
    {
        _fileUploadService = fileUploadService;
        _logger = logger;
        bool isBlockSizeSet = int.TryParse(Environment.GetEnvironmentVariable("RECYCLABLE_STREAM_BLOCK_SIZE") ?? "",
            out int blockSize);
        bool isLargeBufferMultipleSet =
            int.TryParse(Environment.GetEnvironmentVariable("RECYCLABLE_STREAM_BLOCK_LARGE_BUFFER_MULTIPLE") ?? "",
                out int largeBufferMultiple);

        _streamManager = new(
            blockSize: isBlockSizeSet ? blockSize : 80 * 1024, // default 80 KB
            largeBufferMultiple: isLargeBufferMultipleSet ? largeBufferMultiple : 1024 * 1024, // default 1 MB
            maximumBufferSize: int.Parse(Environment.GetEnvironmentVariable("MAX_ALLOWED_PART_SIZE_IN_BYTES")!)
        );
        _streamManager.GenerateCallStacks = true;
        _streamManager.StreamDisposed += (sender, args) =>
        {
            _logger.LogWarning("RMS stream disposed. Tag={Tag}", args.Tag);
        };

        _streamManager.StreamFinalized += (sender, args) =>
        {
            _logger.LogError("RMS stream finalized without Dispose(). Tag={Tag}", args.Tag);
        };

        _streamManager.StreamDoubleDisposed += (sender, args) =>
        {
            _logger.LogWarning("RMS stream disposed twice. Tag={Tag}", args.Tag);
        };

        _streamManager.LargeBufferCreated += (sender, args) =>
        {
            _logger.LogInformation("Large buffer created. Required={RequiredSize}, Pooled={Pooled}, Tag={Tag}",
                args.RequiredSize, args.Pooled, args.Tag);
        };

        // _streamManager.BlockCreated += (sender, args) =>
        // {
        //     _logger.LogDebug("Block created. SmallPoolInUse={SmallPoolInUse}", args.SmallPoolInUse);
        // };
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(50 * 1024 * 1024)] // 50 MB limit for full file upload // todo: include in documentaion
    public async Task<IActionResult> UploadFullFile(
        [FromQuery] string objectKey,
        [FromQuery] string? ownerId,
        [FromForm] IFormFile file
    )
    {
        await using var stream = file.OpenReadStream();

        await _fileUploadService.UploadFullFile(objectKey, stream, ownerId);

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> InitMultipartUpload(
        [FromQuery] string? ownerId,
        [FromBody] InitUploadRequestDto dto
    )
    {
        var result = await _fileUploadService.InitMultipartUpload(
            dto.ObjectKey,
            dto.PartCount,
            dto.PartSizeInBytes,
            dto.Metadata,
            ownerId
        );
        return Ok(result);
    }

    [HttpPost]
    [Consumes("application/octet-stream")]
    public async Task<IActionResult> UploadPart(
        [FromQuery] string uploadId,
        [FromQuery] string? ownerId,
        [FromQuery] int partNumber
    )
    {
        // Optional: upfront Content-Length validation if client sends it
        if (Request.ContentLength.HasValue)
        {
            var maxAllowed = _streamManager.MaximumStreamCapacity;
            if (Request.ContentLength.Value > maxAllowed)
            {
                return StatusCode(StatusCodes.Status413PayloadTooLarge,
                    $"Payload exceeds the maximum allowed size of {maxAllowed} bytes.");
            }
        }

        // Use recyclable memory stream instead of new MemoryStream()
        using (var ms = _streamManager.GetStream($"Upload-{uploadId}-Part-{partNumber}"))
        {
            try
            {
                HttpContext.Features.Get<IHttpMaxRequestBodySizeFeature>()!.MaxRequestBodySize =
                    int.Parse(Environment.GetEnvironmentVariable("MAX_ALLOWED_PART_SIZE_IN_BYTES")!);
                await Request.Body.CopyToAsync(ms);
            }
            catch (InvalidOperationException)
            {
                // Thrown if the stream grows beyond maximumBufferSize
                return StatusCode(StatusCodes.Status413PayloadTooLarge,
                    "Payload exceeded the maximum allowed size.");
            }

            ms.Position = 0; // reset before handing off

            await _fileUploadService.UploadPart(uploadId, partNumber, ms, ownerId);
            return Ok();
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetLeftParts(
        [FromQuery] string uploadId,
        [FromQuery] string? ownerId
    )
    {
        var missingChunks = await _fileUploadService.GetLeftParts(uploadId, ownerId);
        return Ok(missingChunks);
    }

    [HttpPost]
    public async Task<IActionResult> CompleteUpload(
        [FromQuery] string uploadId,
        [FromQuery] string? ownerId
    )
    {
        var result = await _fileUploadService.CompleteUpload(uploadId, ownerId);
        return Ok(new Dictionary<string, dynamic>()
        {
            { "metadata", result }
        });
    }

    [HttpDelete]
    public async Task<IActionResult> AbortUpload(
        [FromQuery] string uploadId,
        [FromQuery] string? ownerId
    )
    {
        await _fileUploadService.AbortUpload(uploadId, ownerId);
        return Ok();
    }
}