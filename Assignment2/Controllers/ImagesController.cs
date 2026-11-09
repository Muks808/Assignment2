using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using Assignment2.Data;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Assignment2.Controllers;

[ApiController]
public sealed class ImagesController(RaceDayStore store, IConfiguration configuration,
    ILogger<ImagesController> logger) : ControllerBase
{
    private int CurrentUserId => int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;

    /// <summary>Uploads a banner for an event owned by the current Organiser.</summary>
    [HttpPost("api/events/{id:int}/banner"), Authorize(Roles = "Organiser")]
    [RequestSizeLimit(5_500_000)]
    public async Task<IActionResult> Banner(int id, IFormFile file, CancellationToken cancellation)
    {
        var race = store.Events.FirstOrDefault(e => e.EventID == id);
        if (race is null) return NotFound();
        if (!store.Organisers.Any(o => o.OrganiserID == race.OrganiserID && o.UserID == CurrentUserId)) return StatusCode(403);

        return await Upload(file, url => race.BannerUrl = url, cancellation);
    }

    /// <summary>Uploads a picture for the authenticated Participant.</summary>
    [HttpPost("api/users/me/image"), Authorize(Roles = "Participant")]
    [RequestSizeLimit(5_500_000)]
    public async Task<IActionResult> Profile(IFormFile file, CancellationToken cancellation)
    {
        var user = store.Users.FirstOrDefault(u => u.UserID == CurrentUserId);
        if (user is null) return NotFound();
        return await Upload(file, url => user.ProfileImageUrl = url, cancellation);
    }

    private async Task<IActionResult> Upload(IFormFile file, Action<string> saveUrl, CancellationToken cancellation)
    {
        if (file.Length is < 8 or > 5_242_880)
            return BadRequest(new { message = "Choose an image between 8 bytes and 5 MB." });

        var signature = new byte[8];
        await using (var header = file.OpenReadStream())
            await header.ReadExactlyAsync(signature, cancellation);

        var extension = signature.AsSpan().SequenceEqual(new byte[] {137, 80, 78, 71, 13, 10, 26, 10})
            ? "png" : signature[0] == 255 && signature[1] == 216 && signature[2] == 255 ? "jpg" : null;
        if (extension is null) return BadRequest(new { message = "Upload a PNG or JPEG image." });

        var connection = configuration.GetConnectionString("RaceDayBlobs");
        if (string.IsNullOrWhiteSpace(connection))
            return StatusCode(503, new { message = "Image storage has not been configured." });

        try
        {
            var container = new BlobContainerClient(connection, configuration["BlobStorage:Container"] ?? "raceday-images");
            await container.CreateIfNotExistsAsync(cancellationToken: cancellation);
            var blob = container.GetBlobClient($"{Guid.NewGuid():N}.{extension}");
            await using var stream = file.OpenReadStream();
            await blob.UploadAsync(stream, new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders { ContentType = extension == "png" ? "image/png" : "image/jpeg" }
            }, cancellation);

            saveUrl(blob.Uri.AbsoluteUri);
            return Ok(new { url = blob.Uri.AbsoluteUri });
        }
        catch (RequestFailedException error)
        {
            logger.LogWarning(error, "Image storage failed.");
            return StatusCode(503, new { message = "Image storage is temporarily unavailable." });
        }
    }

    /// <summary>Streams an event banner or the current user’s own profile picture.</summary>
    [HttpGet("api/images/{kind}/{id:int}")]
    public async Task<IActionResult> Read(string kind, int id, CancellationToken cancellation)
    {
        string? url;
        if (kind == "banner")
            url = (store.Events.FirstOrDefault(e => e.EventID == id))?.BannerUrl;
        else if (kind == "profile" && CurrentUserId == id)
            url = (store.Users.FirstOrDefault(u => u.UserID == id))?.ProfileImageUrl;
        else
            return NotFound();

        var connection = configuration.GetConnectionString("RaceDayBlobs");
        if (url is null || string.IsNullOrWhiteSpace(connection)) return NotFound();

        var container = new BlobContainerClient(connection, configuration["BlobStorage:Container"] ?? "raceday-images");
        var blob = container.GetBlobClient(new Uri(url).Segments[^1]);
        try
        {
            var download = await blob.DownloadStreamingAsync(cancellationToken: cancellation);
            Response.Headers["Cache-Control"] = "private, no-store";
            Response.Headers["X-Content-Type-Options"] = "nosniff";
            return File(download.Value.Content, download.Value.Details.ContentType);
        }
        catch (RequestFailedException error) when (error.Status == 404)
        {
            return NotFound();
        }
        catch (RequestFailedException error)
        {
            logger.LogWarning(error, "Could not read the stored image.");
            return StatusCode(503, new { message = "Image storage is temporarily unavailable." });
        }
    }
}
