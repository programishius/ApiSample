namespace ApiSample.Models
{
    public class Request
    {
        public required string Key { get; set; }
        public required List<object> Values { get; set; }
        public int? ExpireAfterSeconds { get; set; }
    }
}
