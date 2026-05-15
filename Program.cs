var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Day 1 Middleware — Logging
app.Use(async (context, next) =>
{
    Console.WriteLine($"→ REQUEST: {context.Request.Method} {context.Request.Path}");
    await next();
    Console.WriteLine($"← RESPONSE: {context.Response.StatusCode}");
});

app.UseHttpsRedirection();

// ── Day 2: REST API Best Practices ──────────────────────────

// 200 OK — returns list
app.MapGet("/api/v1/experts", () =>
{
    var experts = new[]
    {
        new { Id = 1, Name = "Dr. Ramesh Kumar", Specialization = "Soil Health", Fee = 500 },
        new { Id = 2, Name = "Dr. Priya Nair",   Specialization = "Pest Control", Fee = 400 },
    };
    return Results.Ok(experts); // 200
})
.WithName("GetExperts");

// 200 OK — returns single expert, 404 if not found
app.MapGet("/api/v1/experts/{id}", (int id) =>
{
    if (id == 1)
    {
        var expert = new { Id = 1, Name = "Dr. Ramesh Kumar", Specialization = "Soil Health", Fee = 500 };
        return Results.Ok(expert); // 200
    }
    // 404 - not found
    return Results.NotFound(new { status = 404, message = $"Expert with id {id} not found" });
})
.WithName("GetExpertById");

// 201 Created — create new expert
app.MapPost("/api/v1/experts", (ExpertRequest request) =>
{
    // 400 - validation
    if (string.IsNullOrWhiteSpace(request.Name))
    {
        return Results.BadRequest(new
        {
            status = 400,
            message = "Validation failed",
            errors = new[] { "Name is required" }
        });
    }

    // pretend we saved it and got id = 3
    var created = new { Id = 3, request.Name, request.Specialization, request.Fee };
    return Results.Created($"/api/v1/experts/3", created); // 201
})
.WithName("CreateExpert");

// 204 No Content — delete
app.MapDelete("/api/v1/experts/{id}", (int id) =>
{
    if (id != 1 && id != 2)
    {
        return Results.NotFound(new { status = 404, message = $"Expert {id} not found" });
    }
    return Results.NoContent(); // 204
})
.WithName("DeleteExpert");

app.Run();

// Request model
record ExpertRequest(string Name, string Specialization, int Fee);