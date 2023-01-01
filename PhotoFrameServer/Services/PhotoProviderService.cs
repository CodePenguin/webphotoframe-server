using PhotoFrameServer.Configuration;
using PhotoFrameServer.Core;

namespace PhotoFrameServer.Services;

public class PhotoProviderService
{
    private Dictionary<string, Type> _providerTypes { get; } = new Dictionary<string, Type>();

    public PhotoProviderService(KnownPhotoProviderTypes knownProviderTypes)
    {
        RegisterProviderTypes(knownProviderTypes);
    }

    public Type? GetPhotoProviderType(string providerTypeName)
    {
        if (_providerTypes.TryGetValue(providerTypeName, out var providerType))
        {
            return providerType;
        }
        return _providerTypes.TryGetValue("PhotoFrameServer." + providerTypeName, out providerType)
            ? providerType
            : null;
    }

    private void RegisterProviderType(Type providerType)
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

    private void RegisterProviderTypes(KnownPhotoProviderTypes knownProviderTypes)
    {
        foreach(var providerType in knownProviderTypes)
        {
            RegisterProviderType(providerType);
        }
    }
}
