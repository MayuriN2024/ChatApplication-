# PingMe .NET Backend

This is the C# / .NET 8 conversion of the original Java Spring Boot Chat Application.

## Technology Stack
- **Framework:** ASP.NET Core 8+ Web API
- **Real-time:** SignalR (Replacing STOMP/WebSocket)
- **Database:** Entity Framework Core with MySQL
- **Security:** JWT Bearer Authentication with Cookies
- **Language:** C#

## Project Structure
- `Models/`: Data entities (`User`, `ChatMessage`).
- `Data/`: `ChatDbContext` for database access.
- `Hubs/`: `ChatHub.cs` for real-time messaging logic.
- `Controllers/`: `AuthController` and `MessageController` for RESTful endpoints.
- `Services/`: `AuthService` for business logic and JWT generation.
- `DTOs/`: Data Transfer Objects for requests and responses.

## Key Differences from Java Version
1. **SignalR instead of STOMP:** SignalR is the native and most powerful real-time framework in .NET. It abstracts away protocol details (WebSockets, Long Polling, etc.).
2. **EF Core instead of JPA:** C# uses Entity Framework Core for ORM. It's similar in concept but with different syntax (LINQ).
3. **Program.cs:** Modern .NET uses a single `Program.cs` for both configuration and startup, instead of separate XML or multiple config files.

## How to Run
1. Install [.NET SDK](https://dotnet.microsoft.com/download).
2. Open a terminal in `PingMe.Server`.
3. Run `dotnet restore` to install dependencies.
4. Run `dotnet run` to start the server.
5. The database (`chat.db`) will be automatically created on the first run.

## API Endpoints
- `POST /auth/signup`: User registration.
- `POST /auth/login`: User login (sets JWT cookie).
- `GET /auth/getonlineusers`: List online users.
- `GET /api/messages/private?user1=A&user2=B`: Fetch message history.
- `SignalR Hub`: `/chathub` for real-time connection.
