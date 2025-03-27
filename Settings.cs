namespace ApiSample
{
    public class Settings
    {
        public int DefaultExpirationInSeconds { get; set; }
        public int CleanupIntervalInSeconds { get; set; }
        public int MaxExpirationInSeconds { get; set; }
        public string StorageFilePath { get; set; }
    }
}
