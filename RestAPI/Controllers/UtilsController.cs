using Application.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;
using RestAPI.Controllers.Commons;

namespace RestAPI.Controllers;

public class UtilsController(IFileUtilsService fileUtilsService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetFileSizeInBytes([FromQuery] string objectKey)
    {
        var result = await fileUtilsService.GetFileSizeInBytes(objectKey);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateFileMetadata(
        [FromQuery] string uploadId,
        [FromBody] Dictionary<string, dynamic> metadata
    )
    {
        await fileUtilsService.UpdateFileMetadata(uploadId, metadata);
        return Ok();
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteFiles([FromQuery] List<string> objectKeys)
    {
        var result = await fileUtilsService.DeleteFiles(objectKeys);
        return Ok(result);
    }
}