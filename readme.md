# Introduction to Backend Development with .NET

## Setup Instructions

### Step 1: Create a New Project

To start, create a new project using the command line. Open your terminal and run the following command:

```bash
dotnet new webapi -n blog-website-api
```

### Step 2: Change into your project directory:

```bash
cd blog-website-api
```

### Step 3: Install Dependencies

Install the necessary libraries and dependencies by running the following commands:

```bash
dotnet add package BCrypt.Net-Next --version 4.0.3
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.0.5
dotnet add package Microsoft.AspNetCore.OpenApi --version 8.0.1
dotnet add package Microsoft.EntityFrameworkCore --version 8.0.5
dotnet add package MongoDB.Driver
dotnet add package Pomelo.EntityFrameworkCore.MySql --version 8.0.2
dotnet add package Swashbuckle.AspNetCore --version 6.6.1
dotnet add package Swashbuckle.AspNetCore.Annotations --version 6.6.1
```

### Step 4: Create Folder Structure

Set up the basic folder structure for your project by creating the necessary directories:

```bash
mkdir Controllers
mkdir Data
mkdir DTOs
mkdir Models
```

### Step 5: Create .gitignore file

### Step 6: Initialize git repository

```bash
git init
git add .
git commit -m "initial commit"
git branch -M main
git remote add origin https://github.com/<github-username>/<your-git-repo>.git
git push -u origin main
```

# Coding

## CRUD for users

### Step 1:

```bash
dotnet add package MongoDB.Driver
```

In your project, create a file `User.cs` inside a `Models` folder:

```csharp
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace blog_website_api.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("firstName")]
        public string FirstName { get; set; }

        [BsonElement("lastName")]
        public string LastName { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("password")]
        public string Password { get; set; }

        [BsonElement("role")]
        public string Role { get; set; }
    }
}
```

### Step 2:

Create a file `MongoDbContext.cs` in a `Data` folder:

```csharp
using MongoDB.Driver;
using blog_website_api.Models;

namespace blog_website_api.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetConnectionString("MongoDb"));
            _database = client.GetDatabase("blog-webpage-database");
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("users");
    }
}

```

Update `appsettings.json`:

```bash
"ConnectionStrings": {
    "MongoDb": "mongodb://localhost:27017"
  },
```

### Step 3: Set Up Dependency Injection for MongoDB Context

Register the MongoDB context in your dependency injection container in `Program.cs`:

```bash
using blog_website_api.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<MongoDbContext>(); // This is used to connect to our database with our project
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

```

### Step 4: Create User Controller

Create `UsersController.cs` in the `Controllers` folder:

```csharp
// Located in Controllers/UsersController.cs
using Microsoft.AspNetCore.Mvc;
using blog_website_api.Data;
using blog_website_api.Models;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace blog_website_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public UsersController(MongoDbContext context)
        {
            _context = context;
        }


        // GET: api/users/all
        [HttpGet("all")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users.Find(_ => true).ToListAsync();
            return Ok(users);
        }


        // GET: api/users with pagination
        // to use it in Postman http://localhost:5000/api/users?page=2&pageSize=5
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
    }
}
```

### Step 5: Run the app and test routes with Postman

```bash
dotnet run
```

```bash
{
    "FirstName": "John",
    "LastName": "Doe",
    "Email": "john.doe@example.com",
    "Password": "yourSecurePassword123",
    "Role": "USER"
}
```

## Swagger

dotnet add package Swashbuckle.AspNetCore

In Program.cs import the library:
using Microsoft.OpenApi.Models;

Inside Program.cs update builder.Services.AddSwaggerGen(); to this:

builder.Services.AddSwaggerGen(c =>
{
c.SwaggerDoc("v1", new OpenApiInfo
{
Title = "Blog Website API",
Version = "v1",
Description = "An API for managing users in a blog website.",
Contact = new OpenApiContact
{
Name = "Emir Kugic",
Email = "emir.kugic@stu.ibu.edu.ba",
Url = new Uri("https://www.ibu.edu.ba/")
},

    });
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

});

See swagger: http://localhost:5179/swagger/index.html

Inside your .csproj file add this:

<PropertyGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>

To add custom swagger annotation, go to UsersController.cs and add this:
// GET: api/users with pagination
// to use it in Postman http://localhost:5000/api/users?page=2&pageSize=5
/// <summary>
/// Retrieves all users with pagination.
/// </summary>
/// <param name="page">The page number of the pagination.</param>
/// <param name="pageSize">The number of items per page.</param>
/// <returns>A list of users with pagination information.</returns>
[HttpGet]
