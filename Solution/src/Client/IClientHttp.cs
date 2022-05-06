namespace Client
{
    public interface IClientHttp
    {
        public Task<object> GetAsync(string uri);
    }
}
