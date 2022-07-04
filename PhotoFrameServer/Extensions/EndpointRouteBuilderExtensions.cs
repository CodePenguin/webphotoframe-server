using PhotoFrameServer.Data;
using PhotoFrameServer.Services;
using PhotoFrameServer.ViewModels;

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
        endpoints.MapGet("/photoframe.config.json", async (PhotoFrameRequestHandler handler) =>
            ServePhotoFrame(await handler.GetDefaultPhotoFrameAsync()));

        endpoints.MapGet("/photos/{photoId}", async (Guid photoId, PhotoFrameRequestHandler handler) =>
            ServePhoto(await handler.GetDefaultPhotoFramePhotoAsync(photoId)));

        // Named Photo Frame Endpoints
        endpoints.MapGet("{photoFrameId}/photoframe.config.json", async (string photoFrameId, PhotoFrameRequestHandler handler) =>
            ServePhotoFrame(await handler.GetPhotoFrameAsync(photoFrameId)));

        endpoints.MapGet("{photoFrameId}/photos/{photoId}", async (string photoFrameId, Guid photoId, PhotoFrameRequestHandler handler) =>
            ServePhoto(await handler.GetPhotoAsync(photoFrameId, photoId)));

        return endpoints;
    }

    private static IResult ServePhotoFrame(PhotoFrameModel? photoFrame)
    {
        return photoFrame is not null
            ? Results.Json(photoFrame)
            : Results.NotFound();
    }

    private static IResult ServePhoto(Photo? photo)
    {
        if (photo is null)
        {
            return Results.NotFound();
        }
        return Results.File(photo.FileContents, contentType: GetMimeType(photo.FileExtension));
    }
}
