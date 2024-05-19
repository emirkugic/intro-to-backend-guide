using Microsoft.AspNetCore.Http;

namespace blog_website_api.DTOs.imageDTO.blog_website_api.DTOs
{
    public class BlogUpdateDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public IFormFile? Image { get; set; }
    }
}
