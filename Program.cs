using AgriConsult.API.Data;
using AgriConsult.API.Models;
using AgriConsult.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ── Services ─────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();

// Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token here"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(secretKey))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddScoped<JwtService>();

var app = builder.Build();

// ── Middleware Pipeline ───────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Day 1 — Logging middleware
app.Use(async (context, next) =>
{
    Console.WriteLine($"→ REQUEST: {context.Request.Method} {context.Request.Path}");
    await next();
    Console.WriteLine($"← RESPONSE: {context.Response.StatusCode}");
});

app.UseHttpsRedirection();
app.UseAuthentication(); // ← Must come before UseAuthorization
app.UseAuthorization();

// ── Auth Endpoints ────────────────────────────────────────────

// REGISTER
app.MapPost("/api/auth/register", async (RegisterRequest req, AppDbContext db) =>
{
    // Check if email already exists
    var exists = await db.Users.AnyAsync(u => u.Email == req.Email);
    if (exists)
        return Results.BadRequest(new
        {
            status = 400,
            message = "Email already registered"
        });

    // Hash the password — NEVER store plain text
    var user = new User
    {
        FullName = req.FullName,
        Email = req.Email,
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
        Role = "Farmer"
    };

    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Created($"/api/users/{user.Id}", new
    {
        user.Id,
        user.FullName,
        user.Email,
        user.Role
    });
})
.WithName("Register");

// LOGIN
app.MapPost("/api/auth/login", async (
    LoginRequest req, AppDbContext db, JwtService jwt) =>
{
    // Find user by email
    var user = await db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
    if (user is null)
        return Results.Unauthorized();

    // Verify password
    var validPassword = BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash);
    if (!validPassword)
        return Results.Unauthorized();

    // Generate JWT token
    var token = jwt.GenerateToken(user);

    return Results.Ok(new
    {
        token,
        user.FullName,
        user.Email,
        user.Role,
        expiresIn = "60 minutes"
    });
})
.WithName("Login");

// ── Expert Endpoints (Day 2 + Day 3) ─────────────────────────

// GET all experts — PUBLIC (no auth needed)
app.MapGet("/api/v1/experts", async (AppDbContext db) =>
{
    var experts = await db.Experts.ToListAsync();
    return Results.Ok(experts);
})
.WithName("GetExperts");

// GET single expert — PUBLIC
app.MapGet("/api/v1/experts/{id}", async (int id, AppDbContext db) =>
{
    var expert = await db.Experts.FindAsync(id);
    if (expert is null)
        return Results.NotFound(new
        {
            status = 404,
            message = $"Expert with id {id} not found"
        });
    return Results.Ok(expert);
})
.WithName("GetExpertById");

// POST create expert — PROTECTED (must be logged in)
app.MapPost("/api/v1/experts", async (Expert expert, AppDbContext db,
    ClaimsPrincipal user) =>
{
    if (string.IsNullOrWhiteSpace(expert.Name))
        return Results.BadRequest(new
        {
            status = 400,
            message = "Validation failed",
            errors = new[] { "Name is required" }
        });

    db.Experts.Add(expert);
    await db.SaveChangesAsync();
    return Results.Created($"/api/v1/experts/{expert.Id}", expert);
})
.WithName("CreateExpert")
.RequireAuthorization(); // ← Only logged-in users

// DELETE expert — PROTECTED
app.MapDelete("/api/v1/experts/{id}", async (int id, AppDbContext db) =>
{
    var expert = await db.Experts.FindAsync(id);
    if (expert is null)
        return Results.NotFound(new
        {
            status = 404,
            message = $"Expert {id} not found"
        });
    db.Experts.Remove(expert);
    await db.SaveChangesAsync();
    return Results.NoContent();
})
.WithName("DeleteExpert")
.RequireAuthorization(); // ← Only logged-in users

app.Run();

// ── Request Models ────────────────────────────────────────────
record RegisterRequest(string FullName, string Email, string Password);
record LoginRequest(string Email, string Password);