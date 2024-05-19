using Microsoft.AspNetCore.Mvc;
using blog_website_api.Data;
using MongoDB.Driver;
using Microsoft.AspNetCore.Authorization;
using blog_website_api.Models;
using blog_website_api.DTOs.CommentDTO;

namespace blog_website_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CommentsController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public CommentsController(MongoDbContext context)
        {
            _context = context;
        }


        // GET: api/comments
        [HttpGet]
        public async Task<IActionResult> GetAllComments(int page = 1, int pageSize = 10)
        {
            var totalComments = await _context.Comments.CountDocumentsAsync(_ => true);
            var comments = await _context.Comments.Find(_ => true)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return Ok(new
            {
                TotalCount = totalComments,
                Page = page,
                PageSize = pageSize,
                Comments = comments
            });
        }


        // POST: api/comments
        [HttpPost]
        public async Task<IActionResult> CreateComment([FromBody] CommentDto commentDto)
        {
            // Validate Blog and User existence
            var blogExists = await _context.Blogs.Find(b => b.Id == commentDto.BlogId).AnyAsync();
            if (!blogExists)
            {
                return NotFound($"Blog with ID {commentDto.BlogId} not found.");
            }

            var userExists = await _context.Users.Find(u => u.Id == commentDto.UserId).AnyAsync();
            if (!userExists)
            {
                return NotFound($"User with ID {commentDto.UserId} not found.");
            }

            var comment = new Comment
            {
                BlogId = commentDto.BlogId,
                UserId = commentDto.UserId,
                Content = commentDto.Content,
                DatePosted = DateTime.UtcNow
            };

            await _context.Comments.InsertOneAsync(comment);
            return CreatedAtAction(nameof(GetComment), new { id = comment.Id }, comment);
        }



        // GET: api/comments/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetComment(string id)
        {
            var comment = await _context.Comments.Find(c => c.Id == id).FirstOrDefaultAsync();
            if (comment == null)
            {
                return NotFound();
            }
            return Ok(comment);
        }

        // GET: api/comments/blog/{blogId}
        [HttpGet("blog/{blogId}")]
        public async Task<IActionResult> GetCommentsByBlogId(string blogId, int page = 1, int pageSize = 10)
        {
            var totalComments = await _context.Comments.CountDocumentsAsync(c => c.BlogId == blogId);
            var comments = await _context.Comments.Find(c => c.BlogId == blogId)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return Ok(new
            {
                TotalCount = totalComments,
                Page = page,
                PageSize = pageSize,
                Comments = comments
            });
        }



        // GET: api/comments/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetCommentsByUserId(string userId, int page = 1, int pageSize = 10)
        {
            var userExists = await _context.Users.Find(u => u.Id == userId).AnyAsync();
            if (!userExists)
            {
                return NotFound($"User with ID {userId} not found.");
            }

            var totalComments = await _context.Comments.CountDocumentsAsync(c => c.UserId == userId);
            var comments = await _context.Comments.Find(c => c.UserId == userId)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return Ok(new
            {
                TotalCount = totalComments,
                Page = page,
                PageSize = pageSize,
                Comments = comments
            });
        }


        // PUT: api/comments/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateComment(string id, [FromBody] CommentDto commentDto)
        {
            var existingComment = await _context.Comments.Find(c => c.Id == id).FirstOrDefaultAsync();
            if (existingComment == null)
            {
                return NotFound($"Comment with ID {id} not found.");
            }

            var blogExists = await _context.Blogs.Find(b => b.Id == commentDto.BlogId).AnyAsync();
            if (!blogExists)
            {
                return NotFound($"Blog with ID {commentDto.BlogId} not found.");
            }

            var userExists = await _context.Users.Find(u => u.Id == commentDto.UserId).AnyAsync();
            if (!userExists)
            {
                return NotFound($"User with ID {commentDto.UserId} not found.");
            }

            existingComment.BlogId = commentDto.BlogId;
            existingComment.UserId = commentDto.UserId;
            existingComment.Content = commentDto.Content;
            // Preserve the original posting date
            var result = await _context.Comments.ReplaceOneAsync(c => c.Id == id, existingComment);
            if (result.ModifiedCount == 0)
            {
                return NotFound($"Update failed for Comment with ID {id}.");
            }
            return NoContent();
        }



        // DELETE: api/comments/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteComment(string id)
        {
            var result = await _context.Comments.DeleteOneAsync(c => c.Id == id);
            if (result.DeletedCount == 0)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
