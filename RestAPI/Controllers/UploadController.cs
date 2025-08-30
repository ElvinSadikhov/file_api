using Application.Dtos;
using Application.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;
using RestAPI.Controllers.Commons;

namespace RestAPI.Controllers;

public class UploadController(IFileUploadService fileUploadService) : BaseController
{
    [HttpPost]
    public async Task<IActionResult> Init([FromBody] InitUploadRequestDto dto)
    {
        var result = await fileUploadService.Init(
            dto.ChunkCount,
            dto.ChunkSizeInMb,
            dto.Metadata
        );
        return Ok(result);
    }

    [HttpPost]
    [Consumes("application/octet-stream")]
    public async Task<IActionResult> UploadChunk(
        [FromQuery] string uploadId,
        [FromQuery] int chunkIndex
    )
    {
        using var ms = new MemoryStream();
        await Request.Body.CopyToAsync(ms);
        var chunkData = ms.ToArray();

        await fileUploadService.UploadChunk(uploadId, chunkIndex, chunkData);

        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetLeftChunks([FromQuery] string uploadId)
    {
        var missingChunks = await fileUploadService.GetLeftChunks(uploadId);

        return Ok(missingChunks);
    }

    [HttpPost]
    public async Task<IActionResult> CompleteUpload(
        [FromQuery] string uploadId,
        [FromQuery] string clintChecksum
    )
    {
        await fileUploadService.CompleteUpload(uploadId, clintChecksum);

        return Ok();
    }
}