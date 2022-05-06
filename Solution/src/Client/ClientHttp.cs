namespace Client
{
    public class ClientHttp : IClientHttp
    {
        private readonly HttpClient client = new HttpClient();

        public async Task<HttpResponseMessage> GetAsync(string uri)
        {
            var httpRequestMessage = new HttpRequestMessage();

            httpRequestMessage.RequestUri = new Uri(uri);

            try
            {
                return await client.SendAsync(httpRequestMessage);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}