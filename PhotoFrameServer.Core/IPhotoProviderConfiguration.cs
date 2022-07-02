namespace PhotoFrameServer.Core;

public interface IPhotoProviderConfiguration
{
    public T GetConfiguration<T>() where T : new();
    public T GetInstanceConfiguration<T>() where T : new();
    public void SetConfiguration<T>(T configuration) where T : new();
    public void SetInstanceConfiguration<T>(T configuration) where T : new();
}
