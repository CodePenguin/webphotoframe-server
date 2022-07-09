namespace PhotoFrameServer.Configuration
{
    public class PhotoProviderConfiguration
    {
        public bool Enabled { get; set; } = true;
        public string Id { get; set; } = string.Empty;
        public string ProviderType { get; set; } = string.Empty;
        public Dictionary<string, object> Settings { get; set; } = new();
    }
}
