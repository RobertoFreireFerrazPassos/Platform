using System.Text.Json;

namespace Client
{
    public class ClientHttp : IClientHttp
    {
        private readonly HttpClient client = new HttpClient();

        public async Task<object> GetAsync(string uri)
        {
            var httpRequestMessage = new HttpRequestMessage();

            httpRequestMessage.RequestUri = new Uri(uri);

            try
            {
                var response = await client.SendAsync(httpRequestMessage);
                return JsonSerializer.Deserialize<object>(response.Content.ReadAsStream());                 

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}