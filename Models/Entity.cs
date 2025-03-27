namespace ApiSample.Models
{
    public class Entity
    {
        public required string Key { get; set; }
        public List<object> Values { get; set; } = new List<object>();
        public DateTime? ExpirationTime { get; set; }
    }
}

