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
mkdir Services
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

To install the dependency for swagger, run command:

```bash
dotnet add package Swashbuckle.AspNetCore
```

In `Program.cs` import the library:

```csharp
using Microsoft.OpenApi.Models;
```

Inside Program.cs update `builder.Services.AddSwaggerGen();` to this:

```csharp
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Blog Website API",
        Version = "v1",
        Description = "An API for managing users in a blog website",
        Contact = new OpenApiContact
        {
            Name = "Emir Kugic",
            Email = "emir.kugic@stu.ibu.edu.ba",
            Url = new Uri("https://www.ibu.edu.ba/")
        }
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});
```

To see swagger in your browser visit:

```bash
http://localhost:5179/swagger/index.html
```

Inside your `.csproj` file add this:

```html
<PropertyGroup>
	<GenerateDocumentationFile>true</GenerateDocumentationFile>
	<NoWarn>$(NoWarn);1591</NoWarn>
</PropertyGroup>
```

To add custom swagger annotation, go to `UsersController.cs` and add this:

```csharp
// to use it in Postman http://localhost:5000/api/users?page=2&pageSize=5
// GET: api/users with pagination
/// <summary>
/// Retrieves all users with pagination.
/// </summary>
/// <param name="page">The page number of the pagination.</param>
/// <param name="pageSize">The number of items per page.</param>
/// <returns>A list of users with pagination information.</returns>
[HttpGet];
```

## JWT and Role based authorization

Install libraries:

```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package BCrypt.Net-Next
```

update `appsettings.json`:

```json
"Jwt": {
"Key": "your_super_secret_key_here"
}
```

Inside `Program.cs` import the following:

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
```

And afterwards add this:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
            };
});
```

Update the `AddSwaggerGen` builder to include the following:

```csharp
c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
```

And lastly, at the bottom add:

```csharp
app.UseHttpsRedirection();

app.UseAuthentication(); // This line
app.UseAuthorization(); // And this line
app.MapControllers();

app.Run();
```

Final code should look like this:

```csharp
using blog_website_api.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddSingleton<MongoDbContext>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Blog Website API",
        Version = "v1",
        Description = "An API for managing users in a blog website",
        Contact = new OpenApiContact
        {
            Name = "Emir Kugic",
            Email = "emir.kugic@stu.ibu.edu.ba",
            Url = new Uri("https://www.ibu.edu.ba/")
        }
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
```

Now, let's create custom DTOs for login and register inside `DTOs/AuthDTO/AuthDTO.cs`:

```csharp
namespace blog_website_api.DTOs.AuthDTO

{
public record RegisterDto(string FirstName, string LastName, string Email, string Password);
public record LoginDto(string Email, string Password);
}
```

Now we can build our `AuthController` that's responsible for register and login logic.
Create `AuthController.cs` and add the following code:

```csharp
using Microsoft.AspNetCore.Mvc;
using blog_website_api.Data;
using blog_website_api.Models;
using MongoDB.Driver;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using blog_website_api.DTOs.AuthDTO;

namespace blog_website_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly MongoDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(MongoDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }



        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var userExists = await _context.Users.Find(x => x.Email == registerDto.Email).FirstOrDefaultAsync();
            if (userExists != null)
            {
                return BadRequest("User already exists.");
            }

            var user = new User
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Email = registerDto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                Role = "USER"
            };

            await _context.Users.InsertOneAsync(user);
            return StatusCode(201);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var user = await _context.Users.Find(x => x.Email == loginDto.Email).FirstOrDefaultAsync();
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
            {
                return Unauthorized("Invalid credentials.");
            }

            var token = GenerateJwtToken(user);
            return Ok(new { Token = token });
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role)
                }),
                Expires = DateTime.UtcNow.AddDays(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

    }

}

```

To actually apply authentication role based authorization to your endpoints/routes:

```csharp
using Microsoft.AspNetCore.Mvc;
using blog_website_api.Data;
using blog_website_api.Models;
using MongoDB.Driver;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace blog_website_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public UsersController(MongoDbContext context)
        {
            _context = context;
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
    }
}
```

## Let's create a way to upload images with our backend.

Go to imgur.com and register and log in. (It's free)

Then go to https://api.imgur.com/oauth2/addclient

Add application name and for Authorization type pick the third option: Anonymous usage without user authorization

For Authorization callback URL use http://localhost

You can leave App Website empty.

For email use your own.

Click submit and save client ID and client secret inside appsettings.json:

```json
"Imgur": {
    "ClientId": "2dacf70338ca80e",
    "ClientSecret": "bb7788bbdaf3db86b867dfcc5b07a8fd5ed70c5c"
}
```

Run commands:

```bash
dotnet add package RestSharp
```

```bash
dotnet add package Newtonsoft.Json
```

```bash
dotnet add package Swashbuckle.AspNetCore.Annotations
```

Create `Services/ImgurService.cs`

```csharp
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
```

Implement the service inside `Program.cs` file:

```csharp
builder.Services.AddSingleton<ImgurService>();
```

Inside `Configuration` folder, create a file titled `SwaggerFileOperationFilter.cs` and add this code:

_PS: You can just copy this code, as its boilerplate code. You don't have to memorize it._

```csharp
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class SwaggerFileOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        foreach (var param in operation.Parameters)
        {
            if (param.In == ParameterLocation.Query)
            {
                param.Required = false;
            }
        }

        if (context.ApiDescription.HttpMethod == "POST" || context.ApiDescription.HttpMethod == "PUT")
        {
            if (context.ApiDescription.ParameterDescriptions.Count > 0 && context.ApiDescription.ParameterDescriptions[0].Type == typeof(IFormFile))
            {
                operation.Parameters.Clear();
                operation.RequestBody = new OpenApiRequestBody
                {
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["multipart/form-data"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, OpenApiSchema>
                                {
                                    ["image"] = new OpenApiSchema
                                    {
                                        Type = "string",
                                        Format = "binary"
                                    }
                                }
                            }
                        }
                    }
                };
            }
        }
    }
}
```

Now you can add the configuration file into `Program.cs` by adding this line:
`c.OperationFilter<SwaggerFileOperationFilter>();` inside `AddSecurityRequirement` tag.
Your `Program.cs` file should look like this:

```csharp
using blog_website_api.Data;
using blog_website_api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddSingleton<ImgurService>();
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Blog Website API",
        Version = "v1",
        Description = "An API for managing users in a blog website",
        Contact = new OpenApiContact
        {
            Name = "Emir Kugic",
            Email = "emir.kugic@stu.ibu.edu.ba",
            Url = new Uri("https://www.ibu.edu.ba/")
        }
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
    c.OperationFilter<SwaggerFileOperationFilter>(); //add this line
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
```

Now we can create `ImageDto.cs` inside the `DTOs` folder for uploading our image:

```csharp
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

```

Because we want our user to have an image in its table, we have to update the `User model`:

```csharp
[BsonElement("profileImage")]
public string? ProfileImage { get; set; }

[BsonElement("profileImageDeleteHash")]
public string? ProfileImageDeleteHash { get; set; }
```

Now we can finally create our HTTP routes for uploading images from `UserController`.

First, create an instance of the `ImgurService` and add it to the `UserController constructor`:

```csharp
private readonly MongoDbContext _context;
private readonly ImgurService _imgurService;

public UsersController(MongoDbContext context, ImgurService imgurService)
{
    _context = context;
    _imgurService = imgurService;
}
```

Import everything required:

```csharp
using blog_website_api.Services;
using blog_website_api.DTOs.imageDTO.blog_website_api.DTOs;
using Swashbuckle.AspNetCore.Annotations;
```

The actual routes for uploading

```csharp
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
```

At the end, your UserController should look like this:

```csharp
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
```
