using Microsoft.AspNetCore.Http;

namespace blog_website_api.DTOs.imageDTO.blog_website_api.DTOs
{
    public class BlogCreationDto
    {
        public required string Title { get; set; }
        public required string Description { get; set; }
        public required string UserId { get; set; }
        public required IFormFile Image { get; set; }
    }
}
