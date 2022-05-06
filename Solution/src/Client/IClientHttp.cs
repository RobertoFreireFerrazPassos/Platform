namespace Client
{
    public interface IClientHttp
    {
        public Task<HttpResponseMessage> GetAsync(string uri);
    }
}
