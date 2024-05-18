namespace blog_website_api.DTOs.imageDTO
{

    using Microsoft.AspNetCore.Http;

    namespace blog_website_api.DTOs
    {
        public class ImageDto
        {
            public required IFormFile Image { get; set; }
        }

        public class DeleteImageDto
        {
            public required string DeleteHash { get; set; }
        }
    }

}