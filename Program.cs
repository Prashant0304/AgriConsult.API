using AgriConsult.API.Data;
using AgriConsult.API.Models;
using AgriConsult.API.Services;
using AgriConsult.API.Services.Implementations;
using AgriConsult.API.Services.Interfaces;
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
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IExpertService, ExpertService>();
builder.Services.AddScoped<INotificationService, EmailNotificationService>();


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

// REGISTER — now uses IUserService
app.MapPost("/api/auth/register", async (
    RegisterRequest req, IUserService userService) =>
{
    if (await userService.EmailExistsAsync(req.Email))
        return Results.BadRequest(new
        {
            status = 400,
            message = "Email already registered"
        });

    var user = await userService.CreateUserAsync(
        req.FullName, req.Email, req.Password);

    return Results.Created($"/api/users/{user.Id}", new
    {
        user.Id,
        user.FullName,
        user.Email,
        user.Role
    });
})
.WithName("Register");

// LOGIN — now uses IUserService
app.MapPost("/api/auth/login", async (
    LoginRequest req,
    IUserService userService,
    JwtService jwt,
    INotificationService notification) =>
{
    var user = await userService.GetByEmailAsync(req.Email);
    if (user is null) return Results.Unauthorized();

    var valid = BCrypt.Net.BCrypt.Verify(
        req.Password, user.PasswordHash);
    if (!valid) return Results.Unauthorized();

    // Generate token
    var token = jwt.GenerateToken(user);

    // Send login notification — OCP in action
    // Swap EmailNotification for SMS in DI = zero code change here
    await notification.SendAsync(
        user.Email, $"New login detected for {user.FullName}");

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

// GET experts — now uses IExpertService
app.MapGet("/api/v1/experts", async (IExpertService expertService) =>
{
    var experts = await expertService.GetAllAsync();
    return Results.Ok(experts);
})
.WithName("GetExperts");

// GET expert by id — now uses IExpertService
app.MapGet("/api/v1/experts/{id}", async (
    int id, IExpertService expertService) =>
{
    var expert = await expertService.GetByIdAsync(id);
    if (expert is null)
        return Results.NotFound(new
        {
            status = 404,
            message = $"Expert with id {id} not found"
        });
    return Results.Ok(expert);
})
.WithName("GetExpertById");

// POST expert — now uses IExpertService
app.MapPost("/api/v1/experts", async (
    Expert expert, IExpertService expertService) =>
{
    if (string.IsNullOrWhiteSpace(expert.Name))
        return Results.BadRequest(new
        {
            status = 400,
            message = "Validation failed",
            errors = new[] { "Name is required" }
        });

    var created = await expertService.CreateAsync(expert);
    return Results.Created(
        $"/api/v1/experts/{created.Id}", created);
})
.WithName("CreateExpert")
.RequireAuthorization();

// DELETE expert — now uses IExpertService
app.MapDelete("/api/v1/experts/{id}", async (
    int id, IExpertService expertService) =>
{
    var expert = await expertService.GetByIdAsync(id);
    if (expert is null)
        return Results.NotFound(new
        {
            status = 404,
            message = $"Expert {id} not found"
        });

    await expertService.DeleteAsync(expert);
    return Results.NoContent();
})
.WithName("DeleteExpert")
.RequireAuthorization(); // ← Only logged-in users

app.Run();

// ── Request Models ────────────────────────────────────────────
record RegisterRequest(string FullName, string Email, string Password);
record LoginRequest(string Email, string Password);