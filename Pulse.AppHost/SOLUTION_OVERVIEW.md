# Pulse Aspire Solution - High-Level Overview

**Version:** 1.0  
**Last Updated:** 2027-02-27  
**Target Framework:** .NET 10.0  
**Orchestration:** .NET Aspire 13.2.x  
**Solution Location:** `G:\My Programs\Pulse\Aspire\`

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Solution Architecture](#solution-architecture)
3. [Project Structure](#project-structure)
4. [Aspire Orchestration](#aspire-orchestration)
5. [Technology Stack](#technology-stack)
6. [Data Flow](#data-flow)
7. [Infrastructure Components](#infrastructure-components)
8. [Service Deployments](#service-deployments)
9. [Communication Patterns](#communication-patterns)
10. [Development Workflow](#development-workflow)
11. [Production Considerations](#production-considerations)
12. [System Requirements](#system-requirements)
13. [Getting Started](#getting-started)
14. [Key Features](#key-features)
15. [Future Roadmap](#future-roadmap)

---

## Executive Summary

**Pulse** is a sophisticated, cloud-native **Manufacturing ERP System** built on .NET Aspire, designed for managing industrial roller production, customer relationships, and production planning with advanced AI integration.

### System Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                    PULSE ASPIRE SOLUTION                        │
│                    (.NET 10 / Aspire 13.2)                      │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│   ┌─────────────────────────────────────────────────────────┐   │
│   │  Orchestration Layer (Pulse.AppHost)                    │   │
│   │  • Service discovery & dependency management            │   │
│   │  • Health checks & monitoring                           │   │
│   │  • Docker Compose integration                           │   │
│   └─────────────────────────────────────────────────────────┘   │
│         │                    │                    │             │
│         ▼                    ▼                    ▼             │
│     ┌──────────────┐  ┌──────────────┐  ┌──────────────┐        │
│     │  Pulse.Web   │  │ Pulse.Api    │  │ Pulse.       │        │
│     │              │  │ Service      │  │ DesktopApp   │        │
│     │ (Blazor      │  │              │  │              │        │
│     │  Server)     │  │ (REST API)   │  │ (WinUI3)     │        │
│     └──────────────┘  └──────────────┘  └──────────────┘        │
│         │                    │                    │             │
│         └────────────────────┼────────────────────┘             │
│                              │                                  │
│         ┌────────────────────┴────────────────────┐             │
│         │                      │                  │             │
│         ▼                      ▼                  ▼             │
│     ┌──────────────┐  ┌──────────────┐  ┌──────────────┐        │
│     │ SQL Server   │  │    Redis     │  │   Ollama     │        │
│     │  Database    │  │    Cache     │  │   AI Models  │        │
│     └──────────────┘  └──────────────┘  └──────────────┘        │
│                                                                 │
│    Pulse.MobileApp (.NET MAUI) - Runs independently             │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Key Characteristics

✅ **Cloud-Native Design** - Aspire orchestration with service discovery  
✅ **Multi-Tenant** - Division and company segregation  
✅ **AI-Powered** - Three specialized AI assistants (Ali, Flapper, Tables)  
✅ **Real-Time** - SignalR for live notifications and presence  
✅ **Enterprise** - Role-based access, permission system, audit logs  
✅ **Scalable** - Distributed cache, service discovery, horizontal scaling ready  
✅ **Docker Ready** - Full containerization support  
✅ **Cross-Platform** - Web, Desktop (WinUI3), Mobile (MAUI)

---

## Solution Architecture

### High-Level Architecture Pattern

**Pulse** follows a **Distributed Application Pattern** with clear separation between:

1. **Presentation Layer** - Web (Blazor Server), Desktop (WinUI3), Mobile (MAUI)
2. **API Layer** - RESTful API with JWT authentication, SignalR hubs
3. **Business Logic** - AI services, domain models, business rules
4. **Data Layer** - Entity Framework Core with SQL Server
5. **Infrastructure** - Caching, service discovery, health checks

### Architectural Principles

- **Separation of Concerns** - Each service has clear, focused responsibility
- **Scalability** - Stateless API, distributed caching, load balanceable
- **Resilience** - Health checks, service discovery, error handling
- **Security** - JWT authentication, role-based authorization, HTTPS in production
- **Observability** - OpenTelemetry for tracing, logging, metrics
- **Microservice-Ready** - Can evolve to microservices as needs grow

---

## Project Structure

### Complete Solution Layout

```
Pulse/Aspire/
├── Pulse.AppHost/                          # Aspire Orchestration
│   ├── AppHost.cs                          # Service definitions
│   ├── appsettings.json                    # Configuration
│   ├── appsettings.Development.json        # Dev overrides
│   └── Properties/launchSettings.json      # Launch configuration
│
├── Pulse.ServiceDefaults/                  # Shared infrastructure
│   └── Extensions.cs                       # OpenTelemetry, health checks
│
├── Pulse.Models/                           # Shared domain models
│   ├── PulseContext/                       # EF Core DbContext
│   ├── AI/                                 # AI models
│   ├── Customers/                          # CRM models
│   ├── Production/                         # Manufacturing models
│   ├── Compounds/                          # Material science
│   ├── Users/                              # Identity models
│   ├── Permissions/                        # Authorization models
│   └── ... (50+ DbSets)
│
├── Pulse.ApiService/                       # REST API Backend
│   ├── Program.cs                          # Service configuration
│   ├── Endpoints/                          # API endpoints
│   │   ├── SecurityEndpoints.cs            # Auth/user management
│   │   ├── CustomerEndpoints.cs            # CRM operations
│   │   ├── CompanyEndpoints.cs             # Org structure
│   │   └── ... (15+ endpoint groups)
│   ├── PulseAI/                            # AI integration
│   │   ├── Characters/                     # Ali, Flapper, Tables
│   │   ├── Endpoints/                      # AI API endpoints
│   │   └── Services/                       # AI processing
│   ├── Hubs/                               # SignalR hubs
│   │   └── MessageHub.cs                   # Real-time messaging
│   ├── Migrations/                         # EF Core migrations
│   └── Middleware/                         # Custom middleware
│
├── Pulse.Web/                              # Blazor Server UI
│   ├── Components/                         # Razor components
│   │   ├── Layout/                         # Layout templates
│   │   ├── Pages/                          # Page components
│   │   └── PulseComponents/                # Reusable components
│   ├── Services/                           # Business services
│   │   ├── AuthService.cs                  # Authentication
│   │   ├── PulseApiService.cs              # API client
│   │   ├── Pulse_AI.cs                     # AI orchestration
│   │   └── ... (20+ services)
│   ├── Tools/                              # Utilities
│   ├── wwwroot/                            # Static assets
│   └── Program.cs                          # Service configuration
│
├── Pulse.DesktopApp/                       # WinUI3 Desktop App
│   ├── Views/                              # UI pages
│   ├── Assets/                             # Images/icons
│   └── App.xaml.cs                         # App bootstrap
│
├── Pulse.MobileApp/                        # .NET MAUI Mobile App
│   ├── Platforms/                          # Platform-specific code
│   │   ├── Android/
│   │   ├── iOS/
│   │   ├── MacCatalyst/
│   │   └── Windows/
│   ├── Resources/                          # Styles, fonts, images
│   ├── Services/                           # Business services
│   ├── Pages/                              # XAML pages
│   └── MauiProgram.cs                      # MAUI bootstrap
│
└── ARCHITECTURE.md                         # This document
```

### Project Relationships

```
┌─────────────────────────────────────────────┐
│        Pulse.AppHost (Orchestrator)         │
│  • Defines all services                     │
│  • Service discovery                        │
│  • Health checks                            │
│  • Docker Compose integration               │
└────────────┬────────────────────────────────┘
			 │
	 ┌───────┼───────┬──────────┐
	 │       │       │          │
	 ▼       ▼       ▼          ▼
  ┌──────┐ ┌──────┐ ┌──────┐ ┌──────┐
  │ Web  │ │ Api  │ │ Desk │ │ Mob  │
  └──────┘ └──────┘ └──────┘ └──────┘
	 │       │       │
	 └───┬───┴───┬───┘
		 │       │
		 ▼       ▼
	  ┌──────┐ ┌──────┐
	  │Models│ │Defs  │
	  └──────┘ └──────┘
		 │       │
		 └───┬───┘
			 │
		 ┌───▼────────┐
		 │  Database  │
		 │   Sql DB   │
		 └────────────┘
```

---

## Aspire Orchestration

### Aspire AppHost Overview

The **Pulse.AppHost** project defines the distributed application topology using .NET Aspire 13.2.4.

#### Core Configuration (AppHost.cs)

```csharp
var builder = DistributedApplication.CreateBuilder(args);

// Docker Compose environment
builder.AddDockerComposeEnvironment("compose");

// Redis Cache with Commander UI
var cache = builder.AddRedis("cache")
					.WithRedisCommander()
					.WithDataVolume();

// Database (Development vs Production)
IResourceBuilder<IResourceWithConnectionString> sqlDbResource;

if (builder.Environment.IsDevelopment())
{
	// Local SQL Server connection string
	sqlDbResource = builder.AddConnectionString("dbPulse");
}
else
{
	// Containerized SQL Server 2022
	var sqlPassword = builder.AddParameter("sql-password", secret: true);
	var sql = builder.AddSqlServer("PulseSqlServer", password: sqlPassword, port: 1433)
					 .WithDataVolume("Pulse-Aspire-Sql-Data")
					 .WithEnvironment("ACCEPT_EULA", "Y")
					 .WithEnvironment("MSSQL_PID", "Developer");

	sqlDbResource = sql.AddDatabase("dbPulse");
}

// AI Service Connections
var PulseAI = builder.AddConnectionString("PulseAI");
var TablesAI = builder.AddConnectionString("TablesAI");

// Pulse.ApiService
var PulseApi = builder.AddProject<Projects.Pulse_ApiService>("PulseApi")
	.WithReference(cache)
	.WithReference(sqlDbResource)
	.WithHttpHealthCheck("/alive")
	.WithReference(PulseAI)
	.WithReference(TablesAI)
	.WaitFor(cache);

// Pulse.Web
var PulseWebUI = builder.AddProject<Projects.Pulse_Web>("PulseWebUI")
	.WithExternalHttpEndpoints()
	.WithHttpHealthCheck("/alive")
	.WithReference(PulseApi)
	.WaitFor(PulseApi)
	.WithReference(cache)
	.WaitFor(cache);

// Pulse.DesktopApp
builder.AddProject<Projects.Pulse_DesktopApp>("PulseDesktop")
	.WithReference(PulseApi);

// Note: Pulse.MobileApp runs independently

builder.Build().Run();
```

#### Key Aspire Features Used

1. **Service Discovery** - Automatic DNS resolution for service-to-service communication
2. **Health Checks** - `/alive` endpoint for liveness probes
3. **Dependency Ordering** - `WaitFor()` ensures services start in correct order
4. **Data Volumes** - Persistent storage for Redis and SQL Server
5. **Service References** - Connection string injection via `WithReference()`
6. **Parameter Management** - Secret handling for SQL password
7. **Docker Compose Integration** - Optional Docker Compose orchestration

### Service Dependencies

```
┌─────────────────────────────────────────┐
│         Pulse.Web (Blazor)              │
│                                         │
│  ├─ WaitFor: Pulse.ApiService           │
│  ├─ Reference: Pulse.ApiService         │
│  └─ Reference: Redis Cache              │
└─────────────┬───────────────────────────┘
			  │
			  ▼
┌─────────────────────────────────────────┐
│      Pulse.ApiService (REST API)        │
│                                         │
│  ├─ Reference: SQL Server Database      │
│  ├─ Reference: Redis Cache              │
│  ├─ Reference: PulseAI Connection       │
│  ├─ Reference: TablesAI Connection      │
│  └─ WaitFor: Redis Cache                │
└─────────────┬───────────────────────────┘
			  │
			  ▼
┌─────────────────────────────────────────┐
│         Infrastructure Services         │
│                                         │
│  ├─ SQL Server (Production only)        │
│  ├─ Redis Cache (All environments)      │
│  └─ AI Services (External)              │
└─────────────────────────────────────────┘
```

---

## Technology Stack

### Backend Technologies

| Component | Technology | Version | Purpose |
|-----------|-----------|---------|---------|
| **Runtime** | .NET | 10.0 | Application runtime |
| **Orchestration** | .NET Aspire | 13.2.4 | Service orchestration |
| **Web Framework** | ASP.NET Core | 10.0.7 | HTTP server, API |
| **Database ORM** | Entity Framework Core | 10.0.7 | Data access |
| **Database** | SQL Server | 2022+ | Primary data store |
| **Cache** | Redis (via StackExchange) | - | Distributed caching |
| **Authentication** | ASP.NET Core Identity | 10.0.7 | User auth, roles |
| **APIs** | Minimal APIs | 10.0 | RESTful endpoints |
| **Real-Time** | SignalR | 10.0.7 | WebSocket messaging |
| **AI** | Ollama / OllamaSharp | Latest | Local AI models |
| **JWT** | System.IdentityModel.Tokens.Jwt | 8.17.0 | Token generation/validation |
| **API Docs** | Scalar/Swagger | Latest | Interactive API docs |

### Frontend Technologies (Pulse.Web - Blazor)

| Component | Technology | Version | Purpose |
|-----------|-----------|---------|---------|
| **UI Framework** | Blazor Server | 10.0 | Interactive web UI |
| **Render Mode** | InteractiveServer | 10.0 | Server-side rendering |
| **Component Library** | Microsoft.FluentUI | 4.14.1 | Enterprise components |
| **UI Components** | BootstrapBlazor | 10.5.1 | Bootstrap components |
| **Charts** | Radzen.Blazor | 10.3.1 | Advanced charting |
| **Calendar** | FullCalendar | 6.1.19 | Scheduling component |
| **3D Graphics** | Three.js | Latest | 3D visualization |
| **Styling** | Bootstrap 5 | 5.3.7 | CSS framework |
| **Storage** | Blazor.SessionStorage | 9.0.1 | Browser storage |
| **Speech** | Toolbelt.Blazor.Speech* | 11.0.0 | TTS/STT |
| **Excel Export** | ClosedXML | 0.105.0 | Excel generation |

### Desktop Technologies (Pulse.DesktopApp - WinUI3)

| Component | Technology | Version | Purpose |
|-----------|-----------|---------|---------|
| **Framework** | WinUI 3 | Latest | Windows desktop UI |
| **Runtime** | Windows App SDK | 1.8+ | Windows runtime |
| **Platform** | Windows (x64) | 10+ | Target platform |

### Mobile Technologies (Pulse.MobileApp - MAUI)

| Component | Technology | Version | Purpose |
|-----------|-----------|---------|---------|
| **Framework** | .NET MAUI | 10.0 | Cross-platform mobile |
| **Platforms** | Android, iOS, macOS, Windows | Latest | Target platforms |
| **Navigation** | Shell navigation | Built-in | App navigation |

### Infrastructure & DevOps

| Component | Technology | Version | Purpose |
|-----------|-----------|---------|---------|
| **Containerization** | Docker | Latest | Container images |
| **Orchestration** | Docker Compose (optional) | Latest | Multi-container |
| **Observability** | OpenTelemetry | 1.15.3 | Tracing, metrics |
| **Service Discovery** | Microsoft.Extensions.ServiceDiscovery | 10.5.0 | Dynamic discovery |
| **Resilience** | Polly | 8.6.6 | Retry, timeouts |

---

## Data Flow

### Authentication Flow

```
1. User Login (Browser/Desktop/Mobile)
   ↓
2. POST /Security/login (credentials)
   ↓
3. Pulse.ApiService validates
   ├─ Check ApplicationUser
   ├─ Verify password
   └─ Check roles/permissions
   ↓
4. Generate JWT token
   ├─ Add claims (user ID, roles, permissions)
   ├─ Sign with JWT key
   └─ Set expiration
   ↓
5. Return token + user info
   ↓
6. Client stores token (SessionStorage/LocalStorage/Secure)
   ↓
7. Add Authorization header to subsequent requests
   ├─ GET /api/customers (Header: Authorization: Bearer {token})
   ├─ POST /api/workorders (with JWT)
   └─ WebSocket: /messagehub?access_token={token}
```

### API Request Flow

```
1. Client Request
   ├─ Blazor Component calls PulseApiService
   ├─ HttpClient with AuthHeaderHandler
   └─ Adds JWT token to Authorization header
   ↓
2. Pulse.Web HttpClient → Pulse.ApiService
   ├─ Service discovery: resolve "PulseApi"
   ├─ DNS: PulseApi service endpoint
   └─ HTTPS connection
   ↓
3. Pulse.ApiService Processing
   ├─ JWT validation middleware
   ├─ Route to appropriate endpoint
   ├─ Load user context (claims, roles, permissions)
   ├─ Business logic execution
   ├─ Database query (PulseDbContext)
   └─ Redis cache interaction (if applicable)
   ↓
4. Database (SQL Server)
   ├─ Query execution
   ├─ Data retrieval/modification
   └─ Entity Framework change tracking
   ↓
5. Response Building
   ├─ ApiResponse<T> wrapping
   ├─ JSON serialization (camelCase)
   └─ Status code assignment
   ↓
6. Response to Client
   ├─ HTTP 200 (Success) / 400 (Error) / 401 (Unauthorized)
   ├─ JSON payload with data or errors
   └─ Optional: Cache directives
   ↓
7. Client Handling
   ├─ PulseApiService deserializes to ApiResponse<T>
   ├─ Check Success flag
   ├─ Extract Data property
   └─ Update component state / show toast notification
```

### Real-Time Messaging Flow

```
1. Connection Establishment
   ├─ Client initiates WebSocket to /messagehub
   ├─ Query string includes JWT token
   ├─ Pulse.ApiService MessageHub validates JWT
   └─ Connection established
   ↓
2. Presence Broadcasting
   ├─ User presence updated (Available, Busy, Away, Offline)
   ├─ SignalR broadcasts to all connected clients
   └─ Components receive OnPresenceListUpdated event
   ↓
3. Message Streaming (AI Response)
   ├─ AI generates response (streaming)
   ├─ SignalR sends chunks in real-time
   ├─ Client accumulates chunks
   └─ UI updates with rendered content
   ↓
4. Disconnection Handling
   ├─ Client disconnects
   ├─ MessageHub notifies presence service
   ├─ User marked as Offline
   └─ Broadcasted to other clients
```

### AI Processing Flow

```
1. User Input (Ali, Flapper, or Tables)
   ├─ Blazor component collects input
   ├─ Sends to Pulse.ApiService
   └─ /PulseAI/{Ali|Flapper|Tables}/SendRequest
   ↓
2. AI Service Processing
   ├─ Load schema/context (cached)
   ├─ Format prompt with business context
   ├─ Call Ollama service
   └─ Stream response back
   ↓
3. Ollama Local AI
   ├─ Model inference (local GPU/CPU)
   ├─ Generate response token by token
   ├─ Stream chunks back to service
   └─ Response aggregation
   ↓
4. Response Handling
   ├─ Ali: Store analysis in AnalysisRequest queue
   ├─ Flapper: Save to FlapperMessage/FlapperConversation
   ├─ Tables: Execute query and return results
   └─ Client receives streamed updates
   ↓
5. UI Rendering
   ├─ Stream chunks displayed in real-time
   ├─ Markdown rendering for formatted text
   ├─ Speech synthesis for audio output
   └─ User can interact with results
```

---

## Infrastructure Components

### 1. SQL Server Database

**Purpose:** Primary data store for all business data

**Characteristics:**
- **Version:** SQL Server 2022+ (containerized in production)
- **Location (Dev):** Local or remote connection string
- **Location (Prod):** Container with persistent volume
- **Data Volumes:** `Pulse-Aspire-Sql-Data` for persistence
- **Port:** 1433 (standard SQL Server)

**Databases:**
- `dbPulse` - Main application database with 50+ tables

**Connection Method:**
```csharp
// From Aspire AppHost
builder.AddSqlServerDbContext<PulseDbContext>("dbPulse");
```

### 2. Redis Cache

**Purpose:** Distributed caching for performance and session management

**Characteristics:**
- **Version:** Latest (via Aspire StackExchange.Redis)
- **UI:** Redis Commander on port 8001 (dev only)
- **Data Volumes:** Named volume for persistence
- **Port:** 6379 (default Redis)

**Use Cases:**
- Distributed output caching (Pulse.Web pages)
- Session storage (Blazor circuits)
- AI schema caching (Pulse_AI service)
- User presence tracking

**Reference in AppHost:**
```csharp
var cache = builder.AddRedis("cache")
					.WithRedisCommander()
					.WithDataVolume();
```

### 3. Ollama AI Service

**Purpose:** Local AI model inference for Ali, Flapper, Tables

**Characteristics:**
- **Type:** External service (not orchestrated by Aspire)
- **Endpoint:** Configured via `PulseAI` connection string
- **Default (Development):** `http://192.168.0.6:11434/api/generate`
- **Models:** gpt-oss:latest, mistral, neural-chat, etc.
- **GPU Support:** Optional CUDA acceleration

**Integration:**
```csharp
public class OllamaService
{
	private Uri _localBaseAddress;  // Local Ollama endpoint

	public async IAsyncEnumerable<string> GenerateAsync(string prompt, ...)
}
```

### 4. Service Defaults (Pulse.ServiceDefaults)

**Purpose:** Shared infrastructure configuration for all services

**Includes:**
- OpenTelemetry configuration (logging, tracing, metrics)
- Health checks setup (/health, /alive endpoints)
- Service discovery configuration
- HTTP client defaults for resilience

**Applied to all services via:**
```csharp
builder.AddServiceDefaults();  // In each service's Program.cs
```

---

## Service Deployments

### Pulse.ApiService (REST API Backend)

**Type:** ASP.NET Core Minimal API  
**Port:** 5000 (HTTP), 5001 (HTTPS in production)  
**Health Check:** `/alive` endpoint  
**Dependencies:** SQL Server, Redis, Ollama

**Key Responsibilities:**
- REST API endpoints (Security, Customers, Production, etc.)
- Entity Framework Core data access
- JWT token generation and validation
- SignalR message hub
- AI integration (Ali, Flapper, Tables)
- Business logic and validation
- Email service integration

**Endpoints (Organized):**
```
/Security/*              - Login, register, roles, permissions
/Customers/*             - Client management
/Companies/*             - Company info
/Production/*            - Work orders, planning, stages
/PulseAI/Ali/*          - Analysis AI
/PulseAI/Flapper/*      - Conversational AI
/PulseAI/Tables/*       - Data query AI
/health, /alive         - Health checks
/scalar/v1              - API documentation (Scalar)
```

**Technology Stack:**
- ASP.NET Core 10
- Entity Framework Core 10
- SignalR 10
- OllamaSharp (AI client)
- OpenTelemetry
- Polly (resilience)

### Pulse.Web (Blazor Server UI)

**Type:** Blazor Server Interactive  
**Port:** 5002 (HTTP), 5003 (HTTPS in production)  
**Health Check:** `/alive` endpoint  
**Dependencies:** Pulse.ApiService, Redis

**Key Responsibilities:**
- Interactive Blazor Server UI
- User authentication state management
- Client-side business logic orchestration
- Real-time messaging via SignalR
- AI assistant interfaces
- 3D visualization (Three.js)
- Production dashboards and reporting

**Render Mode:** `@rendermode InteractiveServer`
- Full interactivity without WebAssembly
- Server-side rendering with WebSocket
- SignalR for circuit communication

**Technology Stack:**
- Blazor Server 10
- Fluent UI components
- Bootstrap Blazor
- Radzen components
- Three.js for 3D
- FullCalendar integration

### Pulse.DesktopApp (WinUI3 Desktop)

**Type:** WinUI3 / Windows App SDK  
**Platform:** Windows 10/11 (x64)  
**Dependencies:** Pulse.ApiService (remote)

**Key Responsibilities:**
- Windows desktop application
- Alternative UI to web version
- Same data via REST API
- Potentially offline capability (future)

**Technology Stack:**
- WinUI 3
- Windows App SDK 1.8+
- HttpClient for API calls

### Pulse.MobileApp (.NET MAUI)

**Type:** .NET MAUI (Cross-Platform Mobile)  
**Platforms:** Android, iOS, macOS, Windows  
**Dependencies:** Pulse.ApiService (remote)  
**Deployment:** Independent (not via Aspire)

**Key Responsibilities:**
- Mobile application for iOS and Android
- Same business logic via REST API
- Optimized for mobile UX
- Offline-first patterns (future)

**Technology Stack:**
- .NET MAUI 10
- Native platform integration
- Shell navigation

---

## Communication Patterns

### Synchronous Communication

#### HTTP REST API
- **Pattern:** Request-Response
- **Protocol:** HTTP/HTTPS
- **Format:** JSON
- **Clients:** Pulse.Web, Pulse.DesktopApp, Pulse.MobileApp
- **Authentication:** JWT token in Authorization header

**Example:**
```http
GET https://pulseapi/Customers/GetById?clientId=CUST001
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

Response:
{
  "success": true,
  "data": {
	"fullClientID": "CUST001",
	"clientName": "ABC Industries",
	...
  }
}
```

#### Service-to-Service (Aspire Service Discovery)
- **Pattern:** Direct HTTP calls via DNS
- **Discovery:** Automatic via Aspire
- **Connection:** `http://PulseApi/...` resolves to actual endpoint
- **Clients:** Pulse.Web → Pulse.ApiService

### Asynchronous Communication

#### SignalR WebSocket
- **Pattern:** Pub-Sub / Broadcasting
- **Protocol:** WebSocket (upgrades from HTTP)
- **Clients:** Web browsers (via MessageHubService)
- **Authentication:** JWT token in query string

**Events:**
```csharp
// Server sends to clients
OnReceiveMessage(PulseMessage)        // New message arrived
OnReceiveFlapperChunk(PulseMessage)   // AI chunk received
OnPresenceListUpdated(UserPresenceDto[])  // User presence changed

// Clients send to server
UpdatePresence(PresenceStatus)        // Status change
SendMessage(PulseMessage)             // Send message
```

#### AI Streaming
- **Pattern:** Server-Sent Events / Streaming HTTP
- **Format:** Chunks of JSON
- **Use Case:** Real-time AI response streaming
- **Example:** Flapper response comes in real-time chunks

```csharp
await foreach (var chunk in aiService.GenerateAsync(prompt, cancellationToken))
{
	// Chunk received incrementally
	// Update UI with partial response
}
```

---

## Development Workflow

### Setting Up Development Environment

#### Prerequisites
```
• Visual Studio Community 2026+ or VS Code
• .NET 10 SDK
• SQL Server 2022 or LocalDB
• Docker Desktop (optional, for containerized services)
• Ollama installed locally (for AI features)
• Git
```

#### Running Locally

**Option 1: Using Aspire (Recommended)**

```bash
# Clone repository
git clone https://dev.azure.com/HMTechnologies/Pulse%20-%20Aspire/_git/Pulse
cd Pulse/Aspire

# Set up Aspire AppHost as startup project
# Project: Pulse.AppHost
# Configuration: Debug
# Platform: x64

# Press F5 or Ctrl+F5 to run
# Aspire Dashboard opens: http://localhost:17680
# Services start automatically with dependencies
```

**Option 2: Manual Service Startup**

```bash
# Terminal 1: Start Pulse.ApiService
cd Pulse.ApiService
dotnet run

# Terminal 2: Start Pulse.Web
cd Pulse.Web
dotnet run

# Terminal 3: Ensure SQL Server and Redis are running
# Access: http://localhost:5002 (Pulse.Web)
```

### Development Commands

```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run tests (when implemented)
dotnet test

# Apply EF Core migrations
cd Pulse.ApiService
dotnet ef database update

# Create new migration
dotnet ef migrations add MigrationName

# View API documentation
# After running: http://localhost:5000/scalar/v1

# View Redis Commander
# http://localhost:8001

# View Aspire Dashboard
# http://localhost:17680
```

### Debugging

**Breakpoint Debugging:**
- Set breakpoints in Visual Studio
- Press F5 to run with debugger
- Aspire manages all service startup

**Health Checks:**
- Pulse.Web: `http://localhost:5002/alive`
- Pulse.ApiService: `http://localhost:5000/alive`

**Logging:**
- Output window shows logs from all services
- Filter by service name
- OpenTelemetry traces available in Aspire Dashboard

---

## Production Considerations

### Containerization

**Docker Images:**
```bash
# Build images
docker build -t pulse-api:latest -f Pulse.ApiService/Dockerfile .
docker build -t pulse-web:latest -f Pulse.Web/Dockerfile .

# Run containers
docker run -p 8080:8080 \
  -e ASPNETCORE_URLS=http://+:8080 \
  -e ConnectionStrings__dbPulse=Server=sqlserver;Database=dbPulse \
  pulse-api:latest
```

### Environment Configuration

**Development:**
```json
{
  "Logging": { "LogLevel": { "Default": "Debug" } },
  "PulseApi:Endpoint": "http://localhost:5000",
  "OllamaApi:EndpointHttp": "http://localhost:11434"
}
```

**Production:**
```json
{
  "Logging": { "LogLevel": { "Default": "Warning" } },
  "PulseApi:Endpoint": "https://api.pulse.prod",
  "OllamaApi:EndpointHttp": "https://ai.pulse.prod",
  "Jwt": {
	"Key": "very-long-secret-key",
	"Issuer": "pulse.prod",
	"Audience": "pulse.clients"
  }
}
```

### Deployment Strategies

#### Single Server Deployment
```
nginx/IIS (reverse proxy)
  ├─ Pulse.Web (Blazor Server)
  ├─ Pulse.ApiService (REST API)
  └─ SQL Server + Redis (local or separate VM)
```

#### Kubernetes Deployment (Future)
```
Kubernetes Cluster
  ├─ Pulse.Web Pods (multiple replicas)
  ├─ Pulse.ApiService Pods (multiple replicas)
  ├─ StatefulSet: SQL Server
  ├─ StatefulSet: Redis
  └─ ConfigMap/Secrets: Configuration
```

### Scalability Considerations

**Stateless Design:**
- Pulse.Web: Blazor Server is stateless (circuit per connection)
- Pulse.ApiService: Stateless API (database for state)
- Cache: Redis for distributed state
- Sessions: Redis-backed distributed sessions

**Horizontal Scaling:**
- Load balancer distributes to multiple Pulse.Web instances
- Multiple Pulse.ApiService instances behind load balancer
- Redis shared cache across instances
- SQL Server connection pooling

### Security in Production

**HTTPS Enforcement:**
- All traffic via HTTPS
- HSTS headers enabled
- Certificate management (Let's Encrypt or corporate CA)

**JWT Security:**
- Long secret key (256+ bits)
- Token expiration (e.g., 15 minutes)
- Refresh token for re-authentication
- Issuer/Audience validation

**Database Security:**
- SQL Server authentication (strong passwords)
- Encryption at rest (TDE)
- Encryption in transit (TLS)
- Backup and recovery procedures

**API Security:**
- Rate limiting per API endpoint
- Input validation
- CORS configuration
- CSRF protection

---

## System Requirements

### Development Environment

| Component | Requirement |
|-----------|-------------|
| **OS** | Windows 10/11, macOS 12+, Linux |
| **.NET SDK** | 10.0 |
| **Visual Studio** | 2026+ (Community minimum) or VS Code |
| **RAM** | 16 GB minimum (8 GB with lightweight setup) |
| **Disk Space** | 50 GB (NuGet packages, databases) |
| **SQL Server** | LocalDB or full edition |
| **Redis** | Via Docker or local installation |
| **Docker** | Optional (for containerized services) |
| **Ollama** | Optional (for local AI) |

### Production Environment

| Component | Requirement |
|-----------|-------------|
| **OS** | Windows Server 2019+, Linux (Ubuntu 20.04+), macOS Server |
| **.NET Runtime** | 10.0 |
| **RAM** | 32 GB (varies by user count) |
| **Disk Space** | 500 GB+ (depends on data volume) |
| **SQL Server** | Enterprise or Developer Edition |
| **Redis** | Managed service (Azure Cache) or self-hosted cluster |
| **GPU** | Optional (for AI model acceleration) |
| **Load Balancer** | nginx, IIS ARR, or cloud LB |
| **Backup** | Automated backups (daily minimum) |

---

## Getting Started

### Quick Start (5 minutes)

1. **Clone Repository:**
   ```bash
   git clone https://dev.azure.com/HMTechnologies/Pulse%20-%20Aspire/_git/Pulse
   cd Pulse/Aspire
   ```

2. **Open in Visual Studio:**
   - File → Open Project/Solution
   - Select `Pulse.sln`

3. **Set Pulse.AppHost as Startup:**
   - Right-click Pulse.AppHost
   - Set as Startup Project

4. **Configure Connection Strings:**
   - Update SQL Server connection in appsettings
   - Or use local SQL Server/LocalDB

5. **Run Application:**
   - Press F5 or Debug → Start Debugging
   - Aspire Dashboard: http://localhost:17680
   - Web App: http://localhost:5002

6. **Login:**
   - Default credentials (if seeded):
	 - Username: admin
	 - Password: (check database seed)

### First-Time Setup Steps

1. **Database Migration:**
   ```bash
   cd Pulse.ApiService
   dotnet ef database update
   ```

2. **Seed Initial Data:**
   - Roles: Admin, User
   - Companies, Divisions, Customers
   - Production setup (Work Centers, Equipment)

3. **Configure Ollama (for AI):**
   ```bash
   ollama pull mistral
   ollama serve
   ```

4. **Test API Connectivity:**
   - http://localhost:5000/alive
   - http://localhost:5000/scalar/v1

5. **Verify Authentication:**
   - POST http://localhost:5000/Security/login
   - Body: `{"username":"admin","password":"password"}`

---

## Key Features

### Manufacturing ERP
- **Work Order Management** - Create, track, update production orders
- **Production Planning** - Gantt charts, resource scheduling
- **Inventory Management** - Raw materials, compounds, finished goods
- **Quality Control** - Non-conformance reporting, SPC

### Customer Relationship Management
- **Customer Profiles** - Contact info, specifications, history
- **Sales Management** - Quotations, orders, invoicing
- **Order Tracking** - Real-time status updates
- **Budget Forecasting** - AI-assisted predictions

### AI Integration
- **Ali (Analysis)** - Document analysis, contextual queries
- **Flapper (Chat)** - Natural language conversations
- **Tables (Data)** - SQL generation, data exploration

### Real-Time Collaboration
- **User Presence** - See who's online and their status
- **Live Notifications** - Real-time updates via SignalR
- **Message Hub** - Internal messaging system

### Analytics & Reporting
- **Production Dashboard** - KPIs, capacity planning
- **Customer Dashboard** - Sales trends, top customers
- **Financial Reporting** - Revenue, costs, margins
- **3D Visualization** - Roller specifications in 3D

---

## Future Roadmap

### Short Term (Q2-Q3 2027)
- [ ] Enhanced AI model selection (allow model switching)
- [ ] Advanced reporting with Power BI integration
- [ ] Mobile app UI refinement
- [ ] Offline support for MAUI app

### Medium Term (Q4 2027 - Q1 2028)
- [ ] Microservices refactoring (separate concerns)
- [ ] GraphQL API as alternative to REST
- [ ] Advanced analytics with ML predictions
- [ ] Integration with ERP vendors (SAP, Oracle)

### Long Term (Q2+ 2028)
- [ ] Multi-cloud deployment (Azure, AWS, GCP)
- [ ] IoT sensor integration
- [ ] Supply chain optimization
- [ ] Custom workflow engine
- [ ] Third-party plugin system

---

## Conclusion

**Pulse** is a modern, cloud-native manufacturing ERP system leveraging the latest .NET technologies and patterns. The Aspire-based orchestration provides a solid foundation for:

- **Developer Productivity** - Quick local setup with Aspire
- **Operational Excellence** - Built-in health checks, service discovery, observability
- **Scalability** - Stateless services, distributed caching, load balancing ready
- **Security** - JWT authentication, role-based authorization, modern security practices
- **Extensibility** - AI integration, real-time features, multi-platform support

The architecture balances complexity with maintainability, providing a production-ready foundation while remaining flexible for future enhancements.

---

**Document Version:** 1.0  
**Generated:** 2027-02-27  
**Solution:** Pulse Aspire ERP System  
**Target Framework:** .NET 10.0  
**Orchestration:** .NET Aspire 13.2.x  
**Repository:** https://dev.azure.com/HMTechnologies/Pulse%20-%20Aspire/_git/Pulse  
**Author:** Pulse Development Team
