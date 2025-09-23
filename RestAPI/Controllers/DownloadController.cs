using Application.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;
using RestAPI.Controllers.Commons;

namespace RestAPI.Controllers;

public class DownloadController(IFileDownloadService fileDownloadService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetDownloadUrl([FromQuery] string objectKey)
    {
        var result = await fileDownloadService.GenerateDownloadUrl(objectKey);
        return Ok(result);
    }

    [HttpGet]
    public async Task DownloadPart(
        [FromQuery] string objectKey,
        [FromQuery] int partNumber,
        [FromQuery] long partSizeInBytes,
        [FromQuery] long totalSizeInBytes
    )
    {
        var result = await fileDownloadService.DownloadPart(objectKey, partNumber, partSizeInBytes, totalSizeInBytes);
        Response.ContentType = "application/octet-stream";
        Response.ContentLength = result.Item2;

        await using var stream = result.Item1;
        
        await stream.CopyToAsync(Response.Body);
    }
}