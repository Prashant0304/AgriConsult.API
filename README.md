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
