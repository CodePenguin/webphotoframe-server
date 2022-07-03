namespace PhotoFrameServer.Core
{
    public interface IObjectDictionary : IReadOnlyObjectDictionary
    {
        public new object? this[string key] { get; set; }
    }
}

