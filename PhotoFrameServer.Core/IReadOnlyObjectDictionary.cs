namespace PhotoFrameServer.Core;

public interface IReadOnlyObjectDictionary
{
    public string[] Keys { get; }
    public object? this[string key] { get; }
}

