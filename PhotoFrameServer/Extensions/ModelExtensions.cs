using PhotoFrameServer.Data;
using PhotoFrameServer.ViewModels;

namespace PhotoFrameServer.Extensions;

public static class ModelExtensions
{
    public static PhotoFrameModel ToViewModel(this PhotoFrame photoFrame)
    {
        var model = new PhotoFrameModel();
        foreach (var photo in photoFrame.Photos)
        {
            model.Photos.Add(new PhotoModel
            {
                Caption = photo.Caption,
                Url = $"photos/{photo.PhotoId}"
            });
        }
        return model;
    }
}

