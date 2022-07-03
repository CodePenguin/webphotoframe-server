namespace PhotoFrameServer.Core
{
    public interface IObjectDictionary
    {
        public string[] Keys { get; }
        public object? this[string key] { get; set; }
    }
}

