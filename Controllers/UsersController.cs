using Microsoft.AspNetCore.Mvc;
using blog_website_api.Data;
using blog_website_api.Models;
using MongoDB.Driver;
using Microsoft.AspNetCore.Authorization;
using blog_website_api.Services;
using blog_website_api.DTOs.imageDTO.blog_website_api.DTOs;
using Swashbuckle.AspNetCore.Annotations;

namespace blog_website_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly MongoDbContext _context;
        private readonly ImgurService _imgurService;

        public UsersController(MongoDbContext context, ImgurService imgurService)
        {
            _context = context;
            _imgurService = imgurService;
        }


        // GET: api/users/all
        [HttpGet("all")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users.Find(_ => true).ToListAsync();
            return Ok(users);
        }


        // GET: api/users with pagination
        // to use it in Postman http://localhost:5000/api/users?page=2&pageSize=5
        /// <summary>
        /// Retrieves all users with pagination.
        /// </summary>
        /// <param name="page">The page number of the pagination.</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <returns>A list of users with pagination information.</returns>
        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var usersQuery = _context.Users.Find(_ => true);
            var totalItems = await usersQuery.CountDocumentsAsync();
            var users = await usersQuery.Skip((page - 1) * pageSize).Limit(pageSize).ToListAsync();
            var response = new
            {
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)System.Math.Ceiling(totalItems / (double)pageSize),
                Items = users
            };

            return Ok(response);
        }


        // GET: api/users/5
        [HttpGet("{id}")]
        [Authorize(Roles = "ADMIN,USER")]
        public async Task<IActionResult> GetUser(string id)
        {
            var user = await _context.Users.Find<User>(u => u.Id == id).FirstOrDefaultAsync();
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        // POST: api/users
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] User user)
        {
            await _context.Users.InsertOneAsync(user);
            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        // PUT: api/users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] User updatedUser)
        {
            var result = await _context.Users.ReplaceOneAsync(u => u.Id == id, updatedUser);
            if (result.ModifiedCount == 0)
            {
                return NotFound();
            }
            return NoContent();
        }


        // DELETE: api/users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var result = await _context.Users.DeleteOneAsync(u => u.Id == id);
            if (result.DeletedCount == 0)
            {
                return NotFound();
            }
            return NoContent();
        }



        [HttpPost("{id}/uploadImage")]
        [SwaggerOperation(Summary = "Upload a profile image for the user.")]
        [SwaggerResponse(200, "Image uploaded successfully.", typeof(string))]
        [SwaggerResponse(404, "User not found.")]
        [SwaggerResponse(500, "Image upload failed.")]
        public async Task<IActionResult> UploadImage(string id, [FromForm] ImageDto imageDto)
        {
            var user = await _context.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (user == null)
            {
                return NotFound();
            }

            using var memoryStream = new MemoryStream();
            await imageDto.Image.CopyToAsync(memoryStream);
            var (imageUrl, deleteHash) = await _imgurService.UploadImageAsync(memoryStream.ToArray());
            user.ProfileImage = imageUrl;
            user.ProfileImageDeleteHash = deleteHash;
            await _context.Users.ReplaceOneAsync(u => u.Id == id, user);

            return Ok(new { ImageUrl = imageUrl });
        }

        [HttpDelete("{id}/deleteImage")]
        [SwaggerOperation(Summary = "Delete a profile image for the user.")]
        [SwaggerResponse(204, "Image deleted successfully.")]
        [SwaggerResponse(404, "User not found.")]
        [SwaggerResponse(400, "Image deletion failed.")]
        public async Task<IActionResult> DeleteImage(string id)
        {
            var user = await _context.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (user == null)
            {
                return NotFound();
            }

            if (user.ProfileImageDeleteHash == null)
            {
                return BadRequest("No image to delete.");
            }

            var success = await _imgurService.DeleteImageAsync(user.ProfileImageDeleteHash);
            if (success)
            {
                user.ProfileImage = null;
                user.ProfileImageDeleteHash = null;
                await _context.Users.ReplaceOneAsync(u => u.Id == id, user);
                return NoContent();
            }
            else
            {
                return BadRequest("Image deletion failed.");
            }
        }
    }
}