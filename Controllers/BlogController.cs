using Microsoft.AspNetCore.Mvc;
using blog_website_api.Data;
using blog_website_api.Models;
using MongoDB.Driver;
using Microsoft.AspNetCore.Authorization;
using blog_website_api.Services;
using blog_website_api.DTOs.imageDTO.blog_website_api.DTOs;
using blog_website_api.DTOs;
using Swashbuckle.AspNetCore.Annotations;

namespace blog_website_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BlogsController : ControllerBase
    {
        private readonly MongoDbContext _context;
        private readonly ImgurService _imgurService;

        public BlogsController(MongoDbContext context, ImgurService imgurService)
        {
            _context = context;
            _imgurService = imgurService;
        }

        // POST: api/blogs
        [HttpPost]
        public async Task<IActionResult> CreateBlog([FromForm] BlogCreationDto blogCreationDto)
        {
            var memoryStream = new MemoryStream();
            await blogCreationDto.Image.CopyToAsync(memoryStream);
            var (imageUrl, deleteHash) = await _imgurService.UploadImageAsync(memoryStream.ToArray());

            var blog = new Blog
            {
                Title = blogCreationDto.Title,
                Description = blogCreationDto.Description,
                DateCreated = DateTime.UtcNow,
                ImageUrl = imageUrl,
                ImageDeleteHash = deleteHash,
                UserId = blogCreationDto.UserId
            };

            await _context.Blogs.InsertOneAsync(blog);
            var user = await _context.Users.Find(u => u.Id == blog.UserId).FirstOrDefaultAsync();

            var response = new BlogResponseDto
            {
                Id = blog.Id,
                Title = blog.Title,
                Description = blog.Description,
                DateCreated = blog.DateCreated,
                ImageUrl = blog.ImageUrl,
                UserId = blog.UserId,
                FirstName = user?.FirstName,
                LastName = user?.LastName
            };
            return CreatedAtAction(nameof(GetBlog), new { id = blog.Id }, response);
        }

        // GET: api/blogs/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBlog(string id)
        {
            var blog = await _context.Blogs.Find(b => b.Id == id).FirstOrDefaultAsync();
            if (blog == null)
            {
                return NotFound();
            }

            var user = await _context.Users.Find(u => u.Id == blog.UserId).FirstOrDefaultAsync();
            var response = new BlogResponseDto
            {
                Id = blog.Id,
                Title = blog.Title,
                Description = blog.Description,
                DateCreated = blog.DateCreated,
                ImageUrl = blog.ImageUrl,
                UserId = blog.UserId,
                FirstName = user?.FirstName,
                LastName = user?.LastName
            };
            return Ok(response);
        }


        // GET: api/blogs
        [HttpGet]
        public async Task<IActionResult> GetAllBlogs(int page = 1, int pageSize = 10)
        {
            var totalBlogs = await _context.Blogs.CountDocumentsAsync(_ => true);
            var blogs = await _context.Blogs.Find(_ => true)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            var userIds = blogs.Select(b => b.UserId).Distinct().ToList();
            var users = await _context.Users.Find(u => userIds.Contains(u.Id)).ToListAsync();
            var userDictionary = users.ToDictionary(u => u.Id!, u => new { u.FirstName, u.LastName });

            var response = blogs.Select(b => new BlogResponseDto
            {
                Id = b.Id,
                Title = b.Title,
                Description = b.Description,
                DateCreated = b.DateCreated,
                ImageUrl = b.ImageUrl,
                UserId = b.UserId,
                FirstName = userDictionary[b.UserId].FirstName,
                LastName = userDictionary[b.UserId].LastName
            }).ToList();

            return Ok(new { TotalCount = totalBlogs, Page = page, PageSize = pageSize, Blogs = response });
        }


        // PUT: api/blogs/5
        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Update a blog with a new image or text.")]
        [SwaggerResponse(204, "Blog updated successfully.")]
        public async Task<IActionResult> UpdateBlog(string id, [FromForm] BlogUpdateDto blogUpdateDto)
        {
            var blog = await _context.Blogs.Find(b => b.Id == id).FirstOrDefaultAsync();
            if (blog == null)
            {
                return NotFound();
            }

            if (blogUpdateDto.Image != null)
            {
                var deleteSuccess = await _imgurService.DeleteImageAsync(blog.ImageDeleteHash);
                if (!deleteSuccess)
                {
                    return BadRequest("Failed to delete the old image from Imgur.");
                }

                var memoryStream = new MemoryStream();
                await blogUpdateDto.Image.CopyToAsync(memoryStream);
                var (imageUrl, deleteHash) = await _imgurService.UploadImageAsync(memoryStream.ToArray());

                blog.ImageUrl = imageUrl;
                blog.ImageDeleteHash = deleteHash;
            }

            blog.Title = blogUpdateDto.Title ?? blog.Title;
            blog.Description = blogUpdateDto.Description ?? blog.Description;

            var result = await _context.Blogs.ReplaceOneAsync(b => b.Id == id, blog);
            if (result.ModifiedCount == 0)
            {
                return NotFound();
            }
            return NoContent();
        }

        // DELETE: api/blogs/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBlog(string id)
        {
            var blog = await _context.Blogs.Find(b => b.Id == id).FirstOrDefaultAsync();
            if (blog == null)
            {
                return NotFound();
            }

            var success = await _imgurService.DeleteImageAsync(blog.ImageDeleteHash);
            if (!success)
            {
                return BadRequest("Failed to delete the image from Imgur.");
            }

            var result = await _context.Blogs.DeleteOneAsync(b => b.Id == id);
            if (result.DeletedCount == 0)
            {
                return NotFound();
            }
            return NoContent();
        }

        // GET: api/blogs/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetBlogsByUserId(string userId, int page = 1, int pageSize = 10)
        {
            var totalBlogs = await _context.Blogs.CountDocumentsAsync(b => b.UserId == userId);
            var blogs = await _context.Blogs.Find(b => b.UserId == userId)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            if (!blogs.Any())
            {
                return NotFound("No blogs found for the specified user.");
            }

            var user = await _context.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            var response = blogs.Select(b => new BlogResponseDto
            {
                Id = b.Id,
                Title = b.Title,
                Description = b.Description,
                DateCreated = b.DateCreated,
                ImageUrl = b.ImageUrl,
                UserId = b.UserId,
                FirstName = user?.FirstName,
                LastName = user?.LastName
            }).ToList();

            return Ok(new { TotalCount = totalBlogs, Page = page, PageSize = pageSize, Blogs = response });
        }




        // you have to index the fields you want to search on => first name, last name for users and title for blogs
        [HttpGet("search")]
        public async Task<IActionResult> SearchBlogs(string query, int page = 1, int pageSize = 10)
        {
            // Ensure query is trimmed and processed appropriately if needed
            query = query.Trim();

            // Filter for blog text search
            var filterBlogs = Builders<Blog>.Filter.Text(query);
            var blogQuery = _context.Blogs.Find(filterBlogs);
            var totalBlogs = await blogQuery.CountDocumentsAsync();
            var blogs = await blogQuery.Skip((page - 1) * pageSize).Limit(pageSize).ToListAsync();

            // Filter for user text search
            var filterUsers = Builders<User>.Filter.Text(query);
            var userQuery = _context.Users.Find(filterUsers);
            var users = await userQuery.ToListAsync();
            var userIds = users.Select(u => u.Id).ToList();

            // Fetch additional blogs based on user IDs
            var additionalBlogs = await _context.Blogs.Find(b => userIds.Contains(b.UserId) && !blogs.Any(bg => bg.Id == b.Id)).ToListAsync();
            var combinedBlogs = blogs.Concat(additionalBlogs).Distinct().ToList();

            // Fetch user details for all involved blogs
            var allUserIds = combinedBlogs.Select(b => b.UserId).Distinct().ToList();
            var allUsers = await _context.Users.Find(u => allUserIds.Contains(u.Id)).ToListAsync();
            var userDictionary = allUsers.ToDictionary(u => u.Id!, u => new { u.FirstName, u.LastName });

            // Construct response with user details
            var response = combinedBlogs.Select(b => new BlogResponseDto
            {
                Id = b.Id,
                Title = b.Title,
                Description = b.Description,
                DateCreated = b.DateCreated,
                ImageUrl = b.ImageUrl,
                UserId = b.UserId,
                FirstName = userDictionary.ContainsKey(b.UserId) ? userDictionary[b.UserId].FirstName : "",
                LastName = userDictionary.ContainsKey(b.UserId) ? userDictionary[b.UserId].LastName : ""
            }).ToList();

            return Ok(new { TotalCount = totalBlogs + additionalBlogs.Count, Page = page, PageSize = pageSize, Blogs = response });
        }

    }
}
