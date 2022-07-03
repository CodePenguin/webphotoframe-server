using PhotoFrameServer.Core;

namespace PhotoFrameServer.Services;

public class PhotoProviderService
{
    private Dictionary<string, Type> _providerTypes { get; } = new Dictionary<string, Type>();

    public PhotoProviderService()
    {
    }

    public Type? FindProviderType(string providerTypeName)
    {
        if (_providerTypes.TryGetValue(providerTypeName, out var providerType))
        {
            return providerType;
        }
        return _providerTypes.TryGetValue("PhotoFrameServer." + providerTypeName, out providerType)
            ? providerType
            : null;
    }

    public void RegisterProviderType(Type providerType)
    {
        if (!typeof(IPhotoProvider).IsAssignableFrom(providerType))
        {
            return;
        }
        if (providerType.FullName is null)
        {
            return;
        }
        _providerTypes.TryAdd(providerType.FullName, providerType);
        _providerTypes.TryAdd(providerType.Name, providerType);
    }
}
