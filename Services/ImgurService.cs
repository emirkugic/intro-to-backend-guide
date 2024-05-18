// Services/ImgurService.cs
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace blog_website_api.Services
{
    public class ImgurService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public ImgurService(IConfiguration configuration)
        {
            _configuration = configuration;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://api.imgur.com/3/");
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Client-ID", _configuration["Imgur:ClientId"]);
        }

        public async Task<(string ImageUrl, string DeleteHash)> UploadImageAsync(byte[] imageData)
        {
            using var content = new MultipartFormDataContent();
            content.Add(new ByteArrayContent(imageData), "image");

            var response = await _httpClient.PostAsync("image", content);
            response.EnsureSuccessStatusCode();
            var data = JObject.Parse(await response.Content.ReadAsStringAsync());

            var imageUrl = data["data"]["link"].ToString();
            var deleteHash = data["data"]["deletehash"].ToString();

            return (imageUrl, deleteHash);
        }

        public async Task<bool> DeleteImageAsync(string deleteHash)
        {
            var response = await _httpClient.DeleteAsync($"image/{deleteHash}");
            return response.IsSuccessStatusCode;
        }
    }
}
