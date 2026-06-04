🚀 Day 1: ASP.NET Core Setup & Middleware Basics
📝 Overview
Today’s focus was on initializing the Web API project, downgrading the environment for stability, and understanding the request pipeline.

🛠️ Key Tasks Completed
Environment Configuration: Successfully downgraded the project from .NET 10.0 to .NET 8.0 to ensure compatibility with standard Long-Term Support (LTS) libraries.

Swagger Integration: Replaced the modern .AddOpenApi() syntax with Swashbuckle (Swagger) to provide an interactive documentation UI at /swagger/index.html.

Port Management: Resolved System.IO.IOException (Address already in use) errors by identifying and terminating ghost processes using taskkill.

Middleware Exploration: Experimented with the ASP.NET Core pipeline, including HTTPS redirection and development-mode exception handling.

🧪 Technical Concepts Learned
Middleware: Code components that handle requests and responses in a "checkpoints" system.

The Pipeline Order: Understanding that the order of app.Use... calls in Program.cs matters for security and routing.

Short-circuiting: Using or omitting await next() to control whether a request continues through the pipeline.

🏃 How to Run
Ensure .NET 8 SDK is installed.

Clone the repository and navigate to the project folder.

Run the following commands:

Bash
dotnet restore
dotnet run
Open http://localhost:5245/swagger (or your assigned port) in your browser.

⚠️ Challenges & Solutions
Problem: Failed to bind to address error.

Solution: Used netstat -ano | findstr :5244 to find the process ID and taskkill /F /PID [ID] to free the port.

Problem: Namespace errors for Swagger.

Solution: Added Swashbuckle.AspNetCore NuGet package and updated Program.cs to use .AddSwaggerGen() and .UseSwaggerUI()

Questions:

1. What is middleware in ASP.NET Core?
   It is a piece of code (a component) that sits in the application pipeline to handle incoming requests and outgoing responses. Think of it as a "checkpoint" or a link in a chain that every request must pass through before reaching your controller.

2. What order does it run in?
   It runs in the exact order you define it in Program.cs. It follows a "bidirectional" path: the request goes through the middleware from top to bottom (Request path), and the response travels back through them from bottom to top (Response path).

3. What does await next() do?
   This is the "Pass" command. It tells the current middleware to stop and hand the request over to the next piece of middleware in the pipeline. It ensures the chain stays connected.

4. What happens if I remove await next()?
   The pipeline is "short-circuited." The request stops right there and never reaches the remaining middleware or your API endpoints. The user will likely get a blank response or whatever the current middleware decided to return.

5. Give one real-world example of why middleware is useful
   Authentication/Security: You can have one piece of middleware at the very beginning that checks if a user is logged in. If they aren't, you stop the request immediately, protecting all your API routes at once without writing "check login" code in every single file.

---

## 🚀 Day 2: REST API Best Practices & Status Codes

### 📝 Overview

Today's focus was on building proper REST endpoints with correct HTTP methods,
status codes, error responses, and API versioning.

### 🛠️ Key Tasks Completed

**REST Endpoints Built:** Created CRUD endpoints for the Experts resource
following REST conventions with URL versioning (`/api/v1/experts`)

**Status Codes Implemented:**

- `200 OK` — successful GET request returning existing data
- `201 Created` — successful POST with new resource location header
- `204 No Content` — successful DELETE with nothing to return
- `400 Bad Request` — validation failure with structured error details
- `404 Not Found` — requested resource does not exist in database

**Consistent Error Response Format:**

```json
{
  "status": 400,
  "message": "Validation failed",
  "errors": ["Name is required"]
}
```

**API Versioning:** Implemented URL path versioning (`/api/v1/`) so future
breaking changes can be released as `/api/v2/` without affecting existing clients.

### 🧪 Technical Concepts Learned

**HTTP Methods:**
| Method | Action | Example |
|--------|--------|---------|
| GET | Read | GET /api/v1/experts |
| POST | Create | POST /api/v1/experts |
| PUT | Full Update | PUT /api/v1/experts/5 |
| PATCH | Partial Update | PATCH /api/v1/experts/5 |
| DELETE | Remove | DELETE /api/v1/experts/5 |

**401 vs 403 — Critical Difference:**

- `401 Unauthorized` = Identity problem. Server does not know who you are.
  Token is missing or expired. Fix: log in.
- `403 Forbidden` = Permission problem. Server knows exactly who you are
  but you are not allowed. Fix: you need a different role.

**Decision flow:**
Do I know WHO this person is?
NO → 401 (send them to login)
YES → Are they ALLOWED to do this?
NO → 403 (known but blocked)
YES → process the request

**API Versioning — Why it exists:**
Protects existing clients when you make breaking changes.
Old clients stay on `/api/v1/`, new clients use `/api/v2/`.
Nothing breaks during the transition.

### 🧪 API Endpoints

| Method | Endpoint             | Success Code | Error Codes | Description       |
| ------ | -------------------- | ------------ | ----------- | ----------------- |
| GET    | /api/v1/experts      | 200          | —           | Get all experts   |
| GET    | /api/v1/experts/{id} | 200          | 404         | Get expert by ID  |
| POST   | /api/v1/experts      | 201          | 400         | Create new expert |
| DELETE | /api/v1/experts/{id} | 204          | 404         | Delete expert     |

### ⚠️ Challenges & Solutions

**Problem:** Understanding when to use 201 vs 200
**Solution:** 200 = returning existing data. 201 = something NEW was just
created. Always return the location of the new resource with 201.

**Problem:** Confusing 401 and 403
**Solution:** Ask two questions in order — do I know WHO they are?
Are they ALLOWED? First question fails = 401. Second fails = 403.

### ❓ Questions Answered

**When do you return 201 instead of 200?**
Return 201 specifically on POST requests when a new resource is successfully
created. Include the URL of the new resource in the response. 200 is for
successful reads of existing data.

**What is the difference between 401 and 403?**
401 is an identity problem — the server does not know who you are because
no valid token was provided. 403 is a permission problem — the server knows
who you are but your role does not allow this action. Example: a farmer
accessing the admin dashboard gets 403, not 401.

**Why does API versioning exist?**
To make breaking changes safely. Without versioning, changing your API
format breaks every client using it. With versioning you release v2 while
v1 keeps working. Clients migrate at their own pace.

---

cat >> README.md << 'EOF'

---

### Day 3 — Entity Framework Core & Real Database

**What I built:**

- Connected .NET 8 API to real SQL Server database using Entity Framework Core
- Created Expert entity (C# class → SQL table automatically)
- Wrote AppDbContext as the bridge between C# and database
- Generated database migration using EF Core tools
- Applied migration — AgriConsultDB and Experts table created automatically
- All endpoints now read/write from real SQL Server database

**Proof it works:**

- POST /api/v1/experts → 201 Created → data saved to SQL Server
- GET /api/v1/experts → 200 OK → data returned from real database
- DELETE /api/v1/experts/{id} → 204 No Content → record removed from DB

**Key concepts learned:**

- EF Core is an ORM — maps C# classes to database tables automatically
- DbContext is the bridge between C# and the database
- Migration = snapshot of your model → generates SQL automatically
- `dotnet ef migrations add` = create snapshot
- `dotnet ef database update` = apply to database
- Connection string issues: Encrypt=False needed for local SQL Server dev

**Errors fixed today:**

- Invalid JSON in appsettings.json — nested { } blocks
- SSL certificate error — fixed with Encrypt=False for localhost
- Duplicate route endpoints — removed old hardcoded endpoints
- Class nested inside class in AppDbContext — fixed structure

**How to run:**

```bash
cd AgriConsult.API
ASPNETCORE_ENVIRONMENT=Development dotnet run --no-launch-profile
```

**Connection string (local dev only):**

---

### Day 4 — JWT Authentication

**What I built:**
- User registration endpoint with BCrypt password hashing
- Login endpoint that returns a JWT token
- JWT token validation on protected endpoints
- Swagger configured with Authorize button for testing
- .RequireAuthorization() on POST and DELETE endpoints

**Auth flow tested:**
- POST /api/auth/register → 201 Created
- POST /api/auth/login → 200 OK with JWT token
- POST /api/v1/experts WITH token → 201 Created ✅
- POST /api/v1/experts WITHOUT token → 401 Unauthorized ✅

**Key concepts learned:**
- JWT = header.payload.signature — three parts base64 encoded
- BCrypt hashes passwords — never store plain text
- Claims = data stored inside the token (userId, email, role)
- UseAuthentication must come BEFORE UseAuthorization in pipeline
- .RequireAuthorization() protects individual endpoints

---

## 90-Day Challenge Progress

- [x] Day 1 — Middleware Pipeline ✅
- [x] Day 2 — REST API Best Practices ✅
- [x] Day 3 — Entity Framework Core ✅
- [x] Day 4 — JWT Authentication ✅
- [ ] Day 5 — SOLID Principles

---

### Day 5 — SOLID Principles

**What I learned:**
No code changes today — pure concept learning with AgriConsult examples.

**S — Single Responsibility Principle**
One class should have one reason to change.
AgriConsult example: JwtService only generates tokens. AppDbContext only handles database. Each class has one job.

**O — Open/Closed Principle**
Open for extension, closed for modification.
AgriConsult example: Adding WhatsApp notifications means adding a new class implementing INotificationChannel — EmailNotification and SmsNotification are never touched.

**L — Liskov Substitution Principle**
Child class must be replaceable for parent without breaking anything.
AgriConsult example: EmailNotification and SmsNotification both extend NotificationBase and work correctly wherever NotificationBase is expected.

**I — Interface Segregation Principle**
Classes should not be forced to implement methods they do not need.
AgriConsult example: IAuthService is separate from IProfileService — AuthService is not forced to implement profile methods it never uses.

**D — Dependency Inversion Principle**
Depend on abstractions not concrete implementations. Receive dependencies from outside.
AgriConsult example: JwtService receives IConfiguration through constructor injection. AppDbContext is injected by .NET DI container into endpoints automatically.

**Interview answers prepared for all 5 principles.**

---

## 90-Day Challenge Progress

- [x] Day 1 — Middleware Pipeline ✅
- [x] Day 2 — REST API Best Practices ✅
- [x] Day 3 — Entity Framework Core ✅
- [x] Day 4 — JWT Authentication ✅
- [x] Day 5 — SOLID Principles ✅
- [ ] Day 6 — SOLID Principles in Code
- [ ] Day 7 — Weekend Build: Task Manager API

---

### Day 6 — SOLID Principles in Code

**What I built:**
Refactored AgriConsult to implement all 5 SOLID principles in real C# code.

**Files created:**
- Services/Interfaces/IUserService.cs
- Services/Interfaces/INotificationService.cs
- Services/Interfaces/IExpertService.cs (IExpertReader + IExpertWriter)
- Services/Implementations/UserService.cs
- Services/Implementations/ExpertService.cs
- Services/Implementations/EmailNotificationService.cs
- Services/Implementations/SmsNotificationService.cs

**SOLID applied:**
- S: UserService, ExpertService, JwtService each have ONE job
- O: INotificationService — add WhatsApp by adding new class, zero changes to existing
- L: IExpertReader/IExpertWriter — any implementation substitutable
- I: IExpertReader separate from IExpertWriter — no forced unused methods
- D: All services injected via DI container — endpoints never use `new`

**Result:**
Login endpoint now receives IUserService and INotificationService
through dependency injection. Swap Email for SMS = one line change in DI.
Zero changes to business logic.

---

## 90-Day Challenge Progress

- [x] Day 1 — Middleware Pipeline ✅
- [x] Day 2 — REST API Best Practices ✅
- [x] Day 3 — Entity Framework Core ✅
- [x] Day 4 — JWT Authentication ✅
- [x] Day 5 — SOLID Principles (concepts) ✅
- [x] Day 6 — SOLID Principles in Code ✅
- [ ] Day 7 — Weekend Build
