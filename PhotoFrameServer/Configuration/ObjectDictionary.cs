using PhotoFrameServer.Core;

namespace PhotoFrameServer.Configuration;

public class ObjectDictionary : Dictionary<string, object>, IObjectDictionary, IReadOnlyObjectDictionary
{
    public bool Modified { get; set; }

    public ObjectDictionary() : base()
    {
    }

    public ObjectDictionary(IEnumerable<KeyValuePair<string,object>> collection) : base(collection)
    {
    }

    string[] IObjectDictionary.Keys => Keys.ToArray();
    object? IObjectDictionary.this[string key]
    {
        get => TryGetValue(key, out var value) ? value : null;
        set
        {
            if (TryGetValue(key, out var originalValue) && originalValue != value)
            {
                return;
            }
            if (value is null)
            {
                Remove(key);
            }
            else
            {
                this[key] = value;
            }
            Modified = true;
        }
    }

    string[] IReadOnlyObjectDictionary.Keys => Keys.ToArray();
    object? IReadOnlyObjectDictionary.this[string key] => TryGetValue(key, out var value) ? value : null;
}

