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

    object? IObjectDictionary.this[string key]
    {
        get => TryGetValue(key, out var value) ? value : null;
        set
        {
            if (value is null)
            {
                if (Remove(key))
                {
                    Modified = true;
                };
            }
            else if (!TryGetValue(key, out var originalValue) || !value.Equals(originalValue))
            {
                this[key] = value;
                Modified = true;
            }
        }
    }

    string[] IReadOnlyObjectDictionary.Keys => Keys.ToArray();
    object? IReadOnlyObjectDictionary.this[string key] => TryGetValue(key, out var value) ? value : null;
}

