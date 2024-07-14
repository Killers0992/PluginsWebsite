using Newtonsoft.Json;
using PluginsWebsite.Core.Responses;
using System.Net.Http.Headers;

namespace PluginsWebsite.Client
{
    public static class Extensions
    {
        public static async Task<T> PostAsJsonAsync<T>(this HttpClient client, string requestUri, object content) where T : class
        {
            string text = JsonConvert.SerializeObject(content);

            StringContent stringContent = new StringContent(text, new MediaTypeHeaderValue("application/json"));

            var response = await client.PostAsync(requestUri, stringContent);

            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T>(await response.Content.ReadAsStringAsync());
            }
            else
            {
                return default(T);
            }
        }

        public static async Task<bool> PostAsJsonAsync(this HttpClient client, string requestUri, object content)
        {
            string text = JsonConvert.SerializeObject(content);

            StringContent stringContent = new StringContent(text, new MediaTypeHeaderValue("application/json"));

            var response = await client.PostAsync(requestUri, stringContent);

            return response.IsSuccessStatusCode;
        }
    }
}
