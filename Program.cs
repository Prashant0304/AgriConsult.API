using AgriConsult.API.Data;
using AgriConsult.API.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ── Day 1: Middleware — Logging ──────────────────────────────
app.Use(async (context, next) =>
{
    Console.WriteLine($"→ REQUEST: {context.Request.Method} {context.Request.Path}");
    await next();
    Console.WriteLine($"← RESPONSE: {context.Response.StatusCode}");
});

app.UseHttpsRedirection();

// ── Day 2 + Day 3: Experts Endpoints (Real Database) ────────

// GET all experts — from real database
app.MapGet("/api/v1/experts", async (AppDbContext db) =>
{
    var experts = await db.Experts.ToListAsync();
    return Results.Ok(experts);
})
.WithName("GetExperts");

// GET single expert by id — from real database
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

// POST create expert — saves to real database
app.MapPost("/api/v1/experts", async (Expert expert, AppDbContext db) =>
{
    if (string.IsNullOrWhiteSpace(expert.Name))
    {
        return Results.BadRequest(new
        {
            status = 400,
            message = "Validation failed",
            errors = new[] { "Name is required" }
        });
    }
    db.Experts.Add(expert);
    await db.SaveChangesAsync();
    return Results.Created($"/api/v1/experts/{expert.Id}", expert);
})
.WithName("CreateExpert");

// DELETE expert — removes from real database
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
.WithName("DeleteExpert");

app.Run();