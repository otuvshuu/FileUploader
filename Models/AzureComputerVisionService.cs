using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FileUploader.Models
{
    public class AzureComputerVisionService
    {
        private readonly string _endpoint;
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        public AzureComputerVisionService(string endpoint, string apiKey)
        {
            _endpoint = endpoint;
            _apiKey = apiKey;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _apiKey);
        }

        public async Task<string> DescribeImageAsync(byte[] imageBytes)
        {
            var url = $"{_endpoint}/vision/v3.2/describe";
            using var content = new ByteArrayContent(imageBytes);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var description = doc.RootElement
                .GetProperty("description")
                .GetProperty("captions")[0]
                .GetProperty("text").GetString();
            return description;
        }

        public async Task<(string[] tags, string[] objects)> AnalyzeImageAsync(byte[] imageBytes)
        {
            var url = $"{_endpoint}/vision/v3.2/analyze?visualFeatures=Tags,Objects";
            using var content = new ByteArrayContent(imageBytes);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            var response = await _httpClient.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            var tags = doc.RootElement.TryGetProperty("tags", out var tagsElement)
                ? tagsElement.EnumerateArray().Select(t => t.GetProperty("name").GetString()).ToArray()
                : Array.Empty<string>();

            var objects = doc.RootElement.TryGetProperty("objects", out var objectsElement)
                ? objectsElement.EnumerateArray().Select(o => o.GetProperty("object").GetString()).ToArray()
                : Array.Empty<string>();

            return (tags, objects);
        }
    }
}
