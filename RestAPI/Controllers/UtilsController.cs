using Application.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;
using RestAPI.Controllers.Commons;

namespace RestAPI.Controllers;

public class UtilsController(IFileUtilsService fileUtilsService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetFileSizeInBytes(
        [FromQuery] string objectKey,
        [FromQuery] string? ownerId
    )
    {
        var result = await fileUtilsService.GetFileSizeInBytes(objectKey, ownerId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateRecordMetadata(
        [FromQuery] string uploadId,
        [FromBody] Dictionary<string, dynamic> metadata
    )
    {
        await fileUtilsService.UpdateFileMetadata(uploadId, metadata);
        return Ok();
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteFiles([FromQuery] List<string> objectKeys, [FromQuery] string? ownerId)
    {
        var result = await fileUtilsService.DeleteFiles(objectKeys, ownerId);
        return Ok(result);
    }
}