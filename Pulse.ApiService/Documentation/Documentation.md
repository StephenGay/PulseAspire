# Pulse.ApiService Project Documentation

## Table of Contents
1. [Overview](#overview)
2. [Project Architecture](#project-architecture)
3. [Core Components](#core-components)
4. [Service Configuration](#service-configuration)
5. [API Endpoints](#api-endpoints)
6. [Authentication & Authorization](#authentication--authorization)
7. [Database Context](#database-context)
8. [Real-Time Communication](#real-time-communication)
9. [AI Integration](#ai-integration)
10. [Middleware Pipeline](#middleware-pipeline)
11. [Error Handling](#error-handling)
12. [Configuration & Secrets](#configuration--secrets)
13. [Key Services](#key-services)
14. [Development Notes](#development-notes)

---

## Overview

**Pulse.ApiService** is a comprehensive ASP.NET Core 10 Web API project that serves as the backend for the Pulse enterprise management system. It provides REST APIs, real-time communication via SignalR, AI-powered analysis features, and complete data management for organizational, production, and communication workflows.

### Key Technologies
- **.NET 10** (Latest LTS)
- **ASP.NET Core** (Minimal APIs pattern)
- **Entity Framework Core 10** (SQL Server)
- **SignalR** (Real-time messaging)
- **JWT Authentication** (Bearer tokens)
- **OllamaSharp** (Local AI/LLM integration)
- **Aspire** (Cloud-native applications)
- **Redis** (Distributed caching)

---

## Project Architecture

### Directory Structure

```
Pulse.ApiService/
├── Endpoints/                    # Endpoint definitions (Minimal APIs)
│   ├── SecurityEndpoints.cs     # Authentication/Authorization
│   ├── AiEndpoints.cs           # AI-related operations
│   ├── Production/              # Production management
│   └── [Domain]Endpoints.cs     # Domain-specific endpoints
├── Hubs/                        # SignalR hubs
│   └── MessageHub.cs            # Real-time messaging
├── Middleware/                  # Custom middleware
│   ├── ExceptionHandlingMiddleware.cs
│   ├── RequestLoggingMiddleware.cs
│   ├── SecurityHeadersMiddleware.cs
│   └── RequestLoggingMiddleware.cs
├── Services/                    # Business logic services
│   ├── TokenService.cs
│   ├── EmailService.cs
│   ├── PresenceService.cs
│   └── [Domain]Service.cs
├── PulseAI/                     # AI Integration
│   ├── Characters/              # AI character implementations
│   │   ├── FlapperOllamaAPI.cs
│   │   ├── AliOllamaAPI.cs
│   │   └── TablesAPI.cs
│   ├── Endpoints/               # AI endpoints
│   │   ├── FlapperEndpoints.cs
│   │   └── AliEndpoints.cs
│   └── Services/
│       ├── AliAnalysisWorker.cs # Background analysis service
│       └── TablesSQL.cs
├── Security/                    # Security utilities
│   └── TokenService.cs
├── Extensions/                  # Extension methods
├── Migrations/                  # EF Core migrations
├── Documentation/               # Project documentation
└── Program.cs                   # Application startup
```

---

## Core Components

### 1. **Endpoints (Minimal APIs)**

Pulse.ApiService uses ASP.NET Core's **Minimal APIs** pattern for defining endpoints. Each domain has a dedicated endpoints file.

#### Example Structure:
```csharp
internal static class SecurityEndpoints
{
    internal const string BasePath = "/Security";

    public static void MapSecurityEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/Security");

        group.MapPost("/login", async (LoginModel model, ...) => { ... });
        group.MapPost("/register", async (RegisterModel model, ...) => { ... });
        // ... more endpoints
    }
}
```

#### Endpoint Categories:

| Category | Purpose | Key Endpoints |
|----------|---------|---------------|
| **Security** | Authentication & Authorization | `/login`, `/register`, `/refresh-token` |
| **Users** | User management | `/users/{id}`, `/users/profile` |
| **Divisions** | Division/Department management | `/divisions`, `/divisions/{id}` |
| **Production** | Work orders & scheduling | `/divisions/{id}/GetWIP`, `/production/orders` |
| **Technical** | Technical specifications | `/technical/specs`, `/technical/categories` |
| **AI/Flapper** | AI analysis endpoints | `/ai/flapper/analyze`, `/ai/flapper/chat` |
| **AI/Ali** | Business analysis AI | `/ai/ali/analyze` |
| **Roles** | Role management | `/roles`, `/roles/{id}` |
| **Permissions** | Permission management | `/permissions`, `/permissions/{id}` |

---

## Service Configuration

### Program.cs Initialization Order

#### 1. **Aspire & Infrastructure**
```csharp
builder.AddServiceDefaults();           // Aspire defaults
builder.Services.AddProblemDetails();    // Problem details middleware
builder.AddRedisDistributedCache("cache"); // Redis caching
```

#### 2. **Database Configuration**
```csharp
builder.AddSqlServerDbContext<PulseDbContext>("dbPulse",
    configureDbContextOptions: options =>
    {
        options.UseSqlServer(
        sqlOptions => sqlOptions.MigrationsAssembly("Pulse.ApiService")
        );
    });
builder.Services.AddDbContextFactory<PulseDbContext>(options =>
    options.UseSqlServer());
```

#### 3. **Authentication & Authorization**
```csharp
// JWT Configuration
var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = Encoding.UTF8.GetBytes(
    jwtSettings["Key"] ?? throw new InvalidOperationException("Missing Jwt:Key"));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
    .AddEntityFrameworkStores<PulseDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"] ?? "Pulse.ApiService",
            ValidAudience = jwtSettings["Audience"] ?? "Pulse.Clients",
            IssuerSigningKey = new SymmetricSecurityKey(jwtKey),
            ClockSkew = TimeSpan.FromSeconds(5),
            NameClaimType = JwtRegisteredClaimNames.UniqueName,
            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };

        // Critical for SignalR: Extract JWT from query string
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/messagehub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("User", policy =>
        policy.RequireRole("User", "Admin"));
});
```

#### 4. **CORS Configuration**
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazor", policyBuilder =>
    {
        var blazorOrigin = builder.Configuration["Aspire:Services:PulseWebUI:Http:0"] ?? 
                          "https://localhost:7219";

        policyBuilder
            .WithOrigins(blazorOrigin)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
```

#### 5. **AI/LLM Integration**
```csharp
builder.AddOllamaApiClient("PulseAI").AddChatClient();

// Scoped AI Services
builder.Services.AddScoped<FlapperOllamaAPI>(sp =>
{
    var flapperOllamaClient = sp.GetRequiredService<IOllamaApiClient>();
    var logger = sp.GetRequiredService<ILogger<FlapperOllamaAPI>>();
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    return new FlapperOllamaAPI(flapperOllamaClient, logger, httpClientFactory);
});

builder.Services.AddScoped<AliOllamaAPI>(sp =>
{
    var aliOllamaClient = sp.GetRequiredService<IOllamaApiClient>();
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var logger = sp.GetRequiredService<ILogger<AliOllamaAPI>>();
    return new AliOllamaAPI(aliOllamaClient, logger, httpClientFactory);
});
```

#### 6. **Application Services**
```csharp
builder.Services.AddMemoryCache();
builder.Services.AddScoped<TokenService>();
builder.Services.AddSingleton<PresenceService>();
builder.Services.AddSignalR(options => options.EnableDetailedErrors = true);
builder.Services.AddScoped<UserManager<ApplicationUser>>();
builder.Services.AddSingleton<AiShared>();

// Email Configuration
var emailConfig = builder.Configuration.GetSection("EmailConfiguration");
builder.Services.Configure<EmailConfiguration>(emailConfig);
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPasswordGeneratorService, PasswordGeneratorService>();

// Background Services
builder.Services.AddHostedService<AliAnalysisWorker>();

// AI Services
builder.Services.AddScoped<TablesSQL>();
```

---

## API Endpoints

### Authentication Endpoints
```
POST   /Security/login              - User login (returns JWT)
POST   /Security/register           - User registration
POST   /Security/refresh-token      - Refresh JWT token
POST   /Security/change-password    - Change user password
```

### Production Endpoints
```
GET    /Divisions/{id}/GetWIP       - Get Work In Progress items
POST   /Production/WorkOrders       - Create work order
GET    /Production/WorkOrders/{id}  - Get work order details
PUT    /Production/WorkOrders/{id}  - Update work order
DELETE /Production/WorkOrders/{id}  - Delete work order
```

### AI Endpoints
```
POST   /ai/flapper/analyze          - Flapper AI analysis
POST   /ai/flapper/chat             - Flapper AI chat
POST   /ai/ali/analyze              - Ali business analysis
GET    /ai/ali/status               - Ali analysis status
```

### Data Management Endpoints
```
GET    /Customers                   - List all customers
GET    /Customers/{id}              - Get customer details
POST   /Customers                   - Create customer
PUT    /Customers/{id}              - Update customer
DELETE /Customers/{id}              - Delete customer

GET    /Divisions                   - List all divisions
GET    /Divisions/{id}              - Get division details
POST   /Divisions                   - Create division
PUT    /Divisions/{id}              - Update division
DELETE /Divisions/{id}              - Delete division
```

---

## Authentication & Authorization

### JWT Token Flow

1. **User Login**: `/Security/login`
   - Validates credentials
   - Creates JWT token with claims
   - Returns token to client

2. **Token Structure**:
```json
{
  "iss": "Pulse.ApiService",
  "aud": "Pulse.Clients",
  "sub": "user@example.com",
  "roles": ["User", "Admin"],
  "exp": 1234567890
}
```

3. **Protected Endpoints**: Require `Authorization: Bearer {token}` header

### Authorization Policies

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy => 
        policy.RequireRole("Admin"));

    options.AddPolicy("User", policy => 
        policy.RequireRole("User", "Admin"));
});
```

#### Usage on Endpoints:
```csharp
group.MapPost("/admin/action", AdminAction)
    .RequireAuthorization("Admin");
```

---

## Database Context

### PulseDbContext

Located in `Pulse.Models/PulseContext/PulseDbContext.cs`

#### Main DbSets:
- `Users` - ApplicationUser entities
- `Divisions` - Organizational divisions
- `WorkOrders` - Production work orders
- `Customers` - Client information
- `Products` - Product catalog
- `WorkTypes` - Types of work
- `Roles` - User roles
- `Permissions` - System permissions
- `PulseMessages` - Internal messaging
- `AnalysisRequests` - AI analysis requests
- `ContextualAreas` - Contextual data areas
- `ProductionStages` - Production workflow stages

#### Database Features:
- **Identity Integration**: Uses ASP.NET Core Identity
- **Migrations**: EF Core migrations in `Pulse.ApiService/Migrations`
- **Connection**: Aspire-managed SQL Server connection
- **Factory**: `IDbContextFactory<PulseDbContext>` for scoped access
- **Soft Deletes**: Support for logical deletion
- **Audit Trails**: Tracks modifications via timestamps

---

## Real-Time Communication

### MessageHub (SignalR)

**Location**: `Pulse.ApiService/Hubs/MessageHub.cs`

#### Key Capabilities:
- Real-time messaging between users
- Presence tracking (online/offline)
- Notification broadcasts
- Private and group messages
- Typing indicators

#### Main Methods:

```csharp
// Send private message to specific user
public async Task SendPrivateMessage(PulseMessage message)

// Send group message
public async Task SendGroupMessage(string group, PulseMessage message)

// Get user connection status
public async Task<bool> IsUserOnline(int userId)

// Notify user is typing
public async Task NotifyTyping(string userName)

// Broadcast notification to all connected clients
public async Task BroadcastNotification(Notification notification)
```

#### SignalR Hub Connection

```csharp
app.MapHub<MessageHub>("/messagehub");
```

**JWT Authentication**: JWT is extracted from query string for SignalR authentication.

---

## AI Integration

### 1. **Flapper AI**
- **Purpose**: General-purpose analysis and Q&A
- **Model**: `gpt-oss:latest` (Ollama)
- **Implementation**: `PulseAI/Characters/FlapperOllamaAPI.cs`
- **Endpoints**: `/ai/flapper/*`

#### Key Features:
- Context-aware responses
- Markdown support with HTML conversion
- Table formatting with Material UI styles
- Support for internal links with security attributes

#### Example Usage:
```csharp
var result = await flapperAPI.GenerateAsync(prompt, context);
```

### 2. **Ali AI**
- **Purpose**: Business analysis and insights
- **Model**: `gpt-oss:latest`
- **Implementation**: `PulseAI/Characters/AliOllamaAPI.cs`
- **Background Worker**: `PulseAI/Services/AliAnalysisWorker.cs` (Hosted Service)
- **Endpoints**: `/ai/ali/*`

#### Workflow:
1. Client requests analysis via `/api/ali/analyze`
2. Request queued in database (`AnalysisRequests` table) with status "Queued"
3. `AliAnalysisWorker` polls the queue every 30 seconds
4. Creates scoped dependencies for each analysis
5. Results saved with status "Completed" or "Failed"
6. User notified via SignalR with `PulseMessage`
7. Message saved to database for offline access

#### Background Service Architecture:
```csharp
public class AliAnalysisWorker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Create fresh scope for each iteration
            await using var scope = _scopeFactory.CreateAsyncScope();

            var db = scope.ServiceProvider.GetRequiredService<PulseDbContext>();
            var aliApi = scope.ServiceProvider.GetRequiredService<AliOllamaAPI>();
            var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<MessageHub>>();

            // Poll for queued analysis requests
            var request = await db.AnalysisRequests
                .Where(r => r.Status == "Queued")
                .OrderBy(r => r.QueuedAt)
                .FirstOrDefaultAsync(stoppingToken);

            if (request != null)
            {
                request.Status = "Processing";
                await db.SaveChangesAsync(stoppingToken);

                try
                {
                    // Process analysis
                    var result = await aliApi.AnalyseWithAliAsync(context, entity, userQuery);

                    request.ResultSummary = result.Data;
                    request.Status = "Completed";

                    // Create PulseMessage for user notification
                    var pulseMessage = new PulseMessage
                    {
                        SenderUserName = "Ali",
                        RecipientUserId = request.ApplicationUserId,
                        Subject = "Analysis Complete",
                        Content = result.Data
                    };

                    db.PulseMessages.Add(pulseMessage);
                    await db.SaveChangesAsync(stoppingToken);

                    // Notify via SignalR
                    await hubContext.Clients.User(request.ApplicationUserId.ToString())
                        .SendAsync("ReceiveMessage", pulseMessage, stoppingToken);
                }
                catch (Exception ex)
                {
                    request.Status = "Failed";
                    request.ErrorMessage = ex.Message;
                    await db.SaveChangesAsync(stoppingToken);
                }
            }

            // Wait before next poll
            await Task.Delay(30000, stoppingToken);
        }
    }
}
```

### 3. **TablesAPI**
- **Purpose**: Data analysis and SQL generation
- **Implementation**: `PulseAI/Characters/TablesAPI.cs`, `PulseAI/Services/TablesSQL.cs`
- **Configuration**: Remote Ollama endpoint or local instance

---

## Middleware Pipeline

### Order of Execution

1. **UseServiceDefaults()** - Aspire defaults, logging, tracing
2. **ExceptionHandlingMiddleware** - Global exception handling
3. **SecurityHeadersMiddleware** - Security headers
4. **RequestLoggingMiddleware** - Request/response logging
5. **UseHttpsRedirection()** - HTTPS enforcement
6. **UseCors()** - CORS policy application
7. **UseRouting()** - Route matching
8. **UseAuthentication()** - JWT validation
9. **UseAuthorization()** - Authorization checks
10. **UseWebSockets()** - WebSocket support for SignalR
11. **MapEndpoints()** - Route handlers

### Middleware Details

#### ExceptionHandlingMiddleware
Converts all exceptions to standardized `ApiResponse` format:
```json
{
  "success": false,
  "message": "Error message",
  "statusCode": 500,
  "data": null,
  "timestamp": "2026-05-15T10:30:00Z"
}
```

#### SecurityHeadersMiddleware
Adds security headers:
- `X-Content-Type-Options: nosniff`
- `X-Frame-Options: DENY`
- `X-XSS-Protection: 1; mode=block`
- `Strict-Transport-Security: max-age=31536000; includeSubDomains`
- `Content-Security-Policy: default-src 'self'`

#### RequestLoggingMiddleware
Logs request and response details:
- HTTP method and path
- Query parameters
- Request/response body (when applicable)
- Response status code
- Execution time

---

## Error Handling

### ApiResponse Pattern

All API responses follow the `ApiResponse<T>` pattern:

```csharp
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public int StatusCode { get; set; }
    public DateTime Timestamp { get; set; }
}
```

### Exception Mapping

| Exception Type | HTTP Status | Handler |
|----------------|------------|---------|
| `ArgumentException` | 400 Bad Request | ExceptionHandlingMiddleware |
| `ArgumentNullException` | 400 Bad Request | ExceptionHandlingMiddleware |
| `KeyNotFoundException` | 404 Not Found | ExceptionHandlingMiddleware |
| `UnauthorizedAccessException` | 401 Unauthorized | JWT Bearer |
| `PulseApiException` | Variable | Custom handler |
| `InvalidOperationException` | 400 Bad Request | ExceptionHandlingMiddleware |
| Other exceptions | 500 Internal Server Error | ExceptionHandlingMiddleware |

---

## Configuration & Secrets

### appsettings.json Sections

#### Jwt
```json
{
  "Jwt": {
    "Key": "your-secret-key-here-min-256-bits",
    "Issuer": "Pulse.ApiService",
    "Audience": "Pulse.Clients"
  }
}
```

#### ConnectionStrings
```json
{
  "ConnectionStrings": {
    "dbPulse": "Server=YOUR_SERVER;Database=dbPulse;Trusted_Connection=true;TrustServerCertificate=true;",
    "PulseAI": "Endpoint=http://localhost:11434",
    "RemoteAI": "http://127.0.0.1:11434"
  }
}
```

#### OllamaApi
```json
{
  "OllamaApi": {
    "Key": "api-key-for-ollama",
    "WebSearchEndpoint": "https://ollama.com/api/web_search",
    "WebFetchEndpoint": "https://ollama.com/api/web_fetch"
  }
}
```

#### Aspire Services
```json
{
  "Aspire": {
    "Services": {
      "PulseWebUI": {
        "Http": ["https://localhost:7219"]
      },
      "PulseAI": {
        "Http": ["http://localhost:11434"]
      }
    }
  }
}
```

#### Email Configuration
```json
{
  "EmailConfiguration": {
    "From": "noreply@pulse.local",
    "SmtpServer": "your-smtp-server",
    "SmtpPort": 587,
    "Username": "your-email@domain.com",
    "Password": "your-app-password",
    "EnableSSL": true
  }
}
```

#### Logging
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.EntityFrameworkCore": "Information",
      "Pulse": "Debug"
    }
  }
}
```

### Secrets Management

- **Development**: `appsettings.Development.json` + User Secrets
- **Production**: Environment variables + Azure Key Vault

#### User Secrets (Development Only)
```powershell
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "your-very-long-secret-key-here"
dotnet user-secrets set "EmailConfiguration:Password" "app-password"
```

#### Environment Variables (Production)
```bash
export ASPNETCORE_ENVIRONMENT=Production
export Jwt__Key=your-secret-key
export ConnectionStrings__dbPulse=your-connection-string
export EmailConfiguration__Password=your-email-password
```

---

## Key Services

### TokenService
**Location**: `Pulse.ApiService/Security/TokenService.cs`

Handles JWT token generation and validation:
```csharp
public string GenerateToken(ApplicationUser user, IList<string> roles)
{
    // Creates JWT token with user claims and roles
}

public ClaimsPrincipal? ValidateToken(string token)
{
    // Validates and extracts claims from token
}

public DateTime GetTokenExpiration(string token)
{
    // Returns token expiration time
}
```

### EmailService
**Location**: `Pulse.ApiService/Services/EmailService.cs`

Sends emails with templates:
```csharp
public async Task SendEmailAsync(string to, string subject, string body)
{
    // Sends plain text email
}

public async Task SendTemplateEmailAsync(string to, string templateName, object model)
{
    // Sends email using template with model binding
}

public async Task SendBulkEmailAsync(IEnumerable<string> recipients, string subject, string body)
{
    // Sends bulk emails
}
```

### PresenceService
**Location**: `Pulse.ApiService/Services/PresenceService.cs`

Tracks user online status for real-time features:
```csharp
public void ConnectUser(int userId, string connectionId)
{
    // Registers user connection
}

public void DisconnectUser(string connectionId)
{
    // Removes user connection
}

public IEnumerable<string> GetUserConnectionIds(int userId)
{
    // Gets all connection IDs for a user
}

public bool IsUserOnline(int userId)
{
    // Checks if user has active connections
}
```

### AiShared
**Location**: `Pulse.ApiService/Services/AiShared.cs`

Shared utilities for AI features:
```csharp
public string FormatPrompt(string basePrompt, Dictionary<string, string> context)
{
    // Formats prompt with context variables
}

public string ParseAiResponse(string response)
{
    // Parses and cleans AI response
}
```

---

## Development Notes

### Running the API
```powershell
cd Pulse.ApiService
dotnet run
```

### Running with Aspire Dashboard
```powershell
# From repository root
cd Pulse.AppHost
dotnet run
```

This will start the Aspire dashboard at `https://localhost:17042` and all services.

### Database Migrations
```powershell
# Create migration
dotnet ef migrations add MigrationName

# Apply migrations
dotnet ef database update

# Revert last migration
dotnet ef migrations remove

# Script migrations for deployment
dotnet ef migrations script -o migration.sql
```

### Building Docker Image
```bash
dotnet publish -c Release -o ./publish
docker build -t pulse-api:latest .
docker run -p 7466:8080 pulse-api:latest
```

### Testing Endpoints
- **Scalar UI**: `https://localhost:7466/scalar/v1`
- **OpenAPI JSON**: `https://localhost:7466/openapi/v1.json`
- **Health Check**: `https://localhost:7466/health`

### Common Tasks

#### Add New Endpoint
1. Create method in appropriate `[Domain]Endpoints.cs` file
2. Use `app.MapGroup("/path")` for grouping
3. Add authorization if needed with `.RequireAuthorization()`
4. Return `ApiResponse<T>` from all endpoints

#### Add New Service
1. Create interface in `Services` folder
2. Create implementation class
3. Register in `Program.cs` with appropriate lifetime:
   - `AddSingleton()` - Single instance for application lifetime
   - `AddScoped()` - New instance per request
   - `AddTransient()` - New instance every time

#### Add New Model/Entity
1. Create class in `Pulse.Models` project
2. Add DbSet to `PulseDbContext`
3. Create EF migration: `dotnet ef migrations add AddNewEntity`
4. Apply migration: `dotnet ef database update`

#### Create Background Job
1. Implement `IHostedService` or extend `BackgroundService`
2. Register in `Program.cs` with `AddHostedService<T>()`
3. Use `IServiceScopeFactory` for dependency injection within async loops

---

## Troubleshooting

### Common Issues

#### OllamaApiClient Configuration Error
**Error**: "An OllamaApiClient could not be configured"
- **Solution**: Ensure `ConnectionStrings:PulseAI` is set in appsettings
- **Alternative**: Set `Aspire:OllamaSharp:Endpoint` in configuration
- **AppHost**: Ensure PulseApi has reference to PulseAI service

#### JWT Validation Fails
- Check token expiration time
- Verify JWT key matches in configuration
- Ensure token includes required claims
- Check Authorization header format: `Bearer {token}`

#### SignalR Connection Issues
- Verify CORS is configured correctly
- Check `AllowCredentials()` is enabled
- Ensure JWT is passed in query string for SignalR: `access_token={token}`
- Verify WebSocket support is enabled

#### Database Connection Error
- Check connection string format
- Verify SQL Server is running
- Check firewall settings
- Verify credentials have database access
- Ensure database exists or migration is applied

#### Migrations Not Applied
```powershell
# View pending migrations
dotnet ef migrations list

# Apply all pending migrations
dotnet ef database update

# Update to specific migration
dotnet ef database update MigrationName
```

---

## Performance Considerations

### Optimization Tips

1. **Database Queries**
   - Use `.AsNoTracking()` for read-only queries
   - Use `Select()` to fetch only needed columns
   - Implement pagination for large datasets
   - Use eager loading with `.Include()` to avoid N+1 queries

2. **Caching**
   - Use Redis for distributed caching
   - Cache frequently accessed data (divisions, work types)
   - Set appropriate expiration times
   - Invalidate cache on data updates

3. **SignalR**
   - Limit message size
   - Use compression when appropriate
   - Consider message batching
   - Monitor connection count

4. **AI Requests**
   - Implement timeout for long-running analyses
   - Queue requests to prevent resource exhaustion
   - Use background workers for non-critical analysis
   - Consider rate limiting

---

## Security Best Practices

1. **Authentication**
   - Always require HTTPS in production
   - Use strong JWT keys (256+ bits)
   - Implement token refresh mechanism
   - Set appropriate token expiration

2. **Authorization**
   - Follow least privilege principle
   - Use role-based access control (RBAC)
   - Validate permissions on sensitive operations
   - Audit authorization failures

3. **Input Validation**
   - Validate all user inputs
   - Use model binding validation
   - Sanitize string inputs
   - Prevent SQL injection via EF Core

4. **Data Protection**
   - Use HTTPS for all communications
   - Encrypt sensitive data at rest
   - Use secure headers (CSP, X-Frame-Options, etc.)
   - Implement CORS restrictions

5. **API Security**
   - Implement rate limiting
   - Use API versioning
   - Log security events
   - Monitor for suspicious activity

---

## Deployment

### Prerequisites
- .NET 10 SDK
- SQL Server 2019+
- Redis instance
- Ollama instance (for local AI)

### Production Deployment Checklist
- [ ] Update `appsettings.Production.json` with production settings
- [ ] Set strong JWT key in Azure Key Vault
- [ ] Configure production database connection
- [ ] Set up Redis for distributed caching
- [ ] Enable HTTPS with valid SSL certificate
- [ ] Configure CORS for production domain
- [ ] Set up logging and monitoring
- [ ] Run database migrations
- [ ] Test all endpoints
- [ ] Set up backup strategy

---

## Additional Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
- [SignalR Documentation](https://docs.microsoft.com/en-us/aspnet/core/signalr/)
- [JWT Best Practices](https://tools.ietf.org/html/rfc8949)
- [Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [OllamaSharp GitHub](https://github.com/awaesomee/OllamaSharp)

---

**Last Updated**: May 2026

**Version**: 2.0

This documentation provides a comprehensive overview of the Pulse.ApiService architecture, configuration, and components. For specific implementation details, refer to the inline code documentation and XML comments in the source files.