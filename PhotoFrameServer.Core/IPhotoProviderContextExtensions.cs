using System.Text.Json;

namespace PhotoFrameServer.Core;

public static class IPhotoProviderContextExtensions
{
    public static T GetData<T>(this IPhotoProviderContext context) where T : new()
    {
        return (!string.IsNullOrWhiteSpace(context.Data) ? JsonSerializer.Deserialize<T>(context.Data) : default) ?? new T();
    }

    public static T GetSettings<T>(this IPhotoProviderContext context) where T: new()
    {
        var obj = new T();
        var objType = obj.GetType();
        foreach (var pair in context.Settings)
        {
            var property = objType.GetProperty(pair.Key);
            if (property is not null)
            {
                if (property.PropertyType == typeof(bool))
                {
                    property.SetValue(obj, Convert.ToBoolean(pair.Value), null);
                }
                else
                {
                    property.SetValue(obj, pair.Value, null);
                }
            }
        }
        return obj;
    }

    public static void SetData<T>(this IPhotoProviderContext context, T value) where T : new()
    {
        context.Data = JsonSerializer.Serialize(value);
    }
}
