using Microsoft.EntityFrameworkCore;
using PhotoFrameServer.Data;

namespace PhotoFrameServer.Extensions;

public static class EndpointRouteBuilderExtensions
{
    private static string GetMimeType(string fileExtension)
    {
        return fileExtension switch
        {
            ".bmp" => "image/bmp",
            ".gif" => "image/gif",
            ".jpeg" => "image/jpeg",
            ".jpg" => "image/jpeg",
            ".png" => "image/png",
            ".tif" => "image/tiff",
            ".tiff" => "image/tiff",
            _ => throw new NotImplementedException("Unsupported file extension")
        };
    }

    public static IEndpointRouteBuilder MapPhotoFrameEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // Default Photo Frame Endpoints
        endpoints.MapGet("/photoframe.config.json", async (PhotoFrameContext db) =>
            await db.PhotoFrames.Include(f => f.Photos).FirstOrDefaultAsync() is PhotoFrame photoFrame
                ? Results.Json(photoFrame.ToViewModel())
                : Results.NotFound());

        endpoints.MapGet("/photos/{photoId}", async (Guid photoId, PhotoFrameContext db) =>
            await db.Photos.FindAsync(photoId) is Photo photo
                ? Results.File(photo.FileContents, contentType: GetMimeType(photo.FileExtension))
                : Results.NotFound());

        // Named Photo Frame Endpoints
        endpoints.MapGet("{photoFrameId}/photoframe.config.json", async (string photoFrameId, PhotoFrameContext db) =>
            await db.PhotoFrames.Include(f => f.Photos).FirstOrDefaultAsync(f => f.PhotoFrameId == photoFrameId) is PhotoFrame photoFrame
                ? Results.Json(photoFrame.ToViewModel())
                : Results.NotFound());

        endpoints.MapGet("{photoFrameId}/photos/{photoId}", async (string photoFrameId, Guid photoId, PhotoFrameContext db) =>
            await db.Photos.FindAsync(photoId) is Photo photo
                ? Results.File(photo.FileContents, contentType: GetMimeType(photo.FileExtension))
                : Results.NotFound());

        return endpoints;
    }
}