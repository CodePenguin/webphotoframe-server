using System.Reflection;

namespace PhotoFrameServer.Core;

public static class ObjectDictionaryExtensions
{
    public static T Get<T>(this IReadOnlyObjectDictionary dictionary) where T : class, new()
    {
        var obj = new T();
        var objType = obj.GetType();
        foreach (var key in dictionary.Keys)
        {
            var property = objType.GetProperty(key);
            if (property is not null)
            {
                property.SetValue(obj, dictionary[key], null);
            }
        }
        return obj;
    }

    public static void Set<T>(this IObjectDictionary dictionary, T source) where T: class, new()
    {
        var properties = source.GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
        foreach(var property in properties)
        {
            dictionary[property.Name] = property.GetValue(source, null);
        }
    }
}

