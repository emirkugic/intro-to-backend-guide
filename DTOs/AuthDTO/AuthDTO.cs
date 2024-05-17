namespace blog_website_api.DTOs.AuthDTO

{
    public record RegisterDto(string FirstName, string LastName, string Email, string Password);
    public record LoginDto(string Email, string Password);
}
