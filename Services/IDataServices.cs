namespace ApiSample.Services
{
    public interface IDataServices
    {
        void Create(string key, List<object> values, int expirationSeconds);
        void Append(string key, List<object> values, int expirationSeconds);
        bool Delete(string key);
        bool Get(string key, out List<object> values);
        //void ResetExpiration(string key, int expirationSeconds);
        void CleanupData();
    }
}
