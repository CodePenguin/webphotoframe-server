using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using PhotoFrameServer.Data;
using PhotoFrameServer.ViewModels;

namespace PhotoFrameServer.Extensions;

public static class EndpointRouteBuilderExtensions
{
    private static string GetMimeType(string fileExtension)
    {
        var provider = new FileExtensionContentTypeProvider();
        return provider.TryGetContentType($"file.{fileExtension}", out string? contentType)
            ? contentType
            : "application/octet-stream";
    }

    public static IEndpointRouteBuilder MapPhotoFrameEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // Default Photo Frame Endpoints
        endpoints.MapGet("/photoframe.config.json", async (PhotoFrameContext db) =>
            await db.PhotoFrames.Include(f => f.Photos).FirstOrDefaultAsync() is PhotoFrame photoFrame
                ? Results.Json(photoFrame.ToViewModel())
                : Results.NotFound())
           .Produces<PhotoFrameModel>(StatusCodes.Status200OK)
           .Produces(StatusCodes.Status404NotFound);

        endpoints.MapGet("/photos/{photoId}", async (Guid photoId, PhotoFrameContext db) =>
            await db.Photos.FindAsync(photoId) is Photo photo
                ? Results.File(photo.FileContents, contentType: GetMimeType(photo.FileExtension))
                : Results.NotFound())
           .Produces<PhotoModel>(StatusCodes.Status200OK)
           .Produces(StatusCodes.Status404NotFound);

        // Named Photo Frame Endpoints
        endpoints.MapGet("{photoFrameId}/photoframe.config.json", async (string photoFrameId, PhotoFrameContext db) =>
            await db.PhotoFrames.Include(f => f.Photos).FirstOrDefaultAsync(f => f.PhotoFrameId == photoFrameId) is PhotoFrame photoFrame
                ? Results.Json(photoFrame.ToViewModel())
                : Results.NotFound())
           .Produces<PhotoFrameModel>(StatusCodes.Status200OK)
           .Produces(StatusCodes.Status404NotFound);

        endpoints.MapGet("{photoFrameId}/photos/{photoId}", async (string photoFrameId, Guid photoId, PhotoFrameContext db) =>
            await db.Photos.FindAsync(photoId) is Photo photo
                ? Results.File(photo.FileContents, contentType: GetMimeType(photo.FileExtension))
                : Results.NotFound())
           .Produces<PhotoModel>(StatusCodes.Status200OK)
           .Produces(StatusCodes.Status404NotFound);

        return endpoints;
    }
}