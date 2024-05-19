namespace blog_website_api.DTOs.CommentDTO
{
    public class CommentDto
    {
        public required string BlogId { get; set; }
        public required string UserId { get; set; }
        public required string Content { get; set; }
    }
}