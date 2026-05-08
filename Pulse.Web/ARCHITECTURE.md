# Pulse.Web Project Architecture Documentation

**Version:** 1.0  
**Last Updated:** 2027-02-27  
**Target Framework:** .NET 10.0  
**Application Type:** Blazor Server (Interactive)

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Project Overview](#project-overview)
3. [Architecture Overview](#architecture-overview)
4. [Component Structure](#component-structure)
5. [Service Layer Architecture](#service-layer-architecture)
6. [Authentication & Authorization](#authentication--authorization)
7. [API Communication Layer](#api-communication-layer)
8. [AI Integration Services](#ai-integration-services)
9. [Real-Time Communication](#real-time-communication)
10. [State Management](#state-management)
11. [UI Frameworks & Styling](#ui-frameworks--styling)
12. [Page Structure & Routing](#page-structure--routing)
13. [Configuration Management](#configuration-management)
14. [Dependencies & NuGet Packages](#dependencies--nuget-packages)
15. [Design Patterns & Conventions](#design-patterns--conventions)
16. [Containerization](#containerization)
17. [Future Considerations](#future-considerations)

---

## Executive Summary

**Pulse.Web** is a sophisticated Blazor Server web application serving as the primary user-facing frontend for the Pulse manufacturing ERP system. It provides:

- **Interactive UI** with real-time updates via SignalR
- **Multi-AI Integration** - Three specialized AI assistants (Ali, Flapper, Tables)
- **Complex Business Workflows** - Production management, customer relationship management, AI-assisted analysis
- **Role-Based Access Control** - Granular security with permission management
- **3D Visualization** - Three.js-based 3D roller viewers
- **Advanced Calendaring** - FullCalendar integration for production planning
- **Enterprise UI Components** - Fluent UI, Bootstrap Blazor, Radzen components

---

## Project Overview

### Project Configuration

```xml
<TargetFramework>net10.0</TargetFramework>
<ImplicitUsings>enable</ImplicitUsings>
<Nullable>enable</Nullable>
<UserSecretsId>ed233e63-d309-4d34-ab58-3804fc7b934e</UserSecretsId>
```

### Project Type
- **SDK:** Microsoft.NET.Sdk.Web
- **Application Model:** Blazor Server with InteractiveServer render mode
- **Platform:** ASP.NET Core 10 with WebAssembly support
- **Deployment:** Docker containerization ready

### Primary Responsibilities
1. Render interactive user interfaces with Blazor components
2. Manage client-side application state and circuit handling
3. Communicate with Pulse.ApiService via HTTP
4. Provide real-time features through SignalR messaging hub
5. Handle JWT-based authentication and authorization
6. Integrate with Ollama AI for natural language processing
7. Manage user presence and notifications

### Project Dependencies
- **Pulse.Models** - Shared domain models
- **Pulse.ApiService** - Backend API client
- **Pulse.ServiceDefaults** - Cross-cutting configuration

---

## Architecture Overview

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────┐
│              Browser / Client                           │
│         (HTML, CSS, JavaScript, WebAssembly)            │
└─────────────────────────────────────────────────────────┘
						  │
						  ▼
┌─────────────────────────────────────────────────────────┐
│         Pulse.Web (Blazor Server Application)           │
│   ┌──────────────────────────────────────────────────┐  │
│   │     Razor Components (.razor files)              │  │
│   │  (Pages, Layouts, Custom Components)             │  │
│   └──────────────────────────────────────────────────┘  │
│   ┌──────────────────────────────────────────────────┐  │
│   │     Service Layer                                │  │
│   │  (Auth, API, AI, State Management)               │  │
│   └──────────────────────────────────────────────────┘  │
│   ┌──────────────────────────────────────────────────┐  │
│   │     Tools & Utilities                            │  │
│   │  (AppState, AiModel, GlobalFunctions)            │  │
│   └──────────────────────────────────────────────────┘  │
│   ┌──────────────────────────────────────────────────┐  │
│   │     SignalR Hub Connection                       │  │
│   │  (Real-time messaging & notifications)           │  │
│   └──────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
		│                      │                    │
		▼                      ▼                    ▼
┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐
│  Pulse.ApiService│ │  Ollama AI       │ │  SignalR Hub     │
│  (REST API)      │ │  (Local/Cloud)   │ │  (Real-time)     │
└──────────────────┘ └──────────────────┘ └──────────────────┘
		│
		▼
┌──────────────────┐
│  SQL Server      │
│  Database        │
└──────────────────┘
```

### Architectural Layers

#### 1. **Presentation Layer** (Razor Components)
- Interactive Blazor Server components with `@rendermode InteractiveServer`
- Real-time interactivity without full page reloads
- Responsive UI with CSS scoped styling

#### 2. **Service Layer** (Core Business Logic)
- **Authentication Services** - JWT token management, authentication state
- **API Services** - HTTP communication with backend
- **AI Services** - Integration with multiple AI assistants
- **Real-Time Services** - SignalR message hub for live notifications
- **State Management** - Circuit-scoped and application-scoped state

#### 3. **Data Access Layer**
- Indirect: Via PulseApiService (REST API calls)
- No direct database access in web layer

#### 4. **Cross-Cutting Concerns**
- Logging (ILogger)
- Caching (Redis via Aspire)
- Configuration management
- Error handling and validation

---

## Component Structure

### Component Organization

```
Pulse.Web/Components/
├── App.razor                              # Root component
├── Routes.razor                           # Route configuration
├── _Imports.razor                         # Global imports
│
├── Layout/                                # Layout templates
│   ├── MasterLayout.razor                 # Primary layout
│   ├── MasterLayout.razor.css             # Scoped styles
│   ├── MasterAIMenu.razor                 # AI assistant menu
│   ├── NoLayout.razor                     # Layout-less pages
│   ├── EmployeeZoneMainLayout.razor       # Employee area
│   ├── EmployeeZoneNavMenu.razor          # Employee navigation
│   ├── AdminZoneLayout.razor              # Admin area
│   ├── ProductionLayout.razor             # Production dashboard
│   ├── EZ-CustomersLayout.razor           # Customer management
│   └── ProductionSetupLayout.razor        # Setup pages
│
├── Pages/                                 # Page components
│   ├── Home.razor                         # Landing page
│   ├── Error.razor                        # Error boundary
│   │
│   ├── Security/                          # Auth pages
│   │   ├── Login.razor
│   │   ├── LoginToDivisionPg.razor
│   │   ├── ResetPassword.razor
│   │   └── Access-Denied.razor
│   │
│   ├── AiZone/                            # AI assistants
│   │   ├── AliPg.razor                    # Analysis AI
│   │   ├── FlapperPg.razor                # Conversational AI
│   │   ├── TablesPg.razor                 # Structured data AI
│   │   └── FlapperSettings.razor          # AI configuration
│   │
│   ├── AdminZone/                         # Administration
│   │   ├── AdminDashboard.razor
│   │   ├── UserManagementPg.razor
│   │   ├── RoleManagementPg.razor
│   │   ├── PermissionManagementPg.razor
│   │   ├── IntergrationManagement.razor
│   │   ├── SiteSensorManagement.razor
│   │   └── TokenDebugPage.razor
│   │
│   ├── EmployeeZone/                      # Main application area
│   │   ├── EmployeeZoneHome.razor
│   │   │
│   │   ├── Customers/                     # CRM pages
│   │   │   ├── ClientDashboard.razor
│   │   │   ├── ClientProfile.razor
│   │   │   ├── ClientBudgetPg.razor
│   │   │   ├── ClientContacts.razor
│   │   │   ├── ClientSales.razor
│   │   │   ├── ClientRollerSpecs.razor
│   │   │   ├── 3DRoller.razor
│   │   │   ├── RollerSpecificationDetail.razor
│   │   │   └── Dialogs/
│   │   │       ├── AiBudgetCreator.razor
│   │   │       └── UserPresenceList.razor
│   │   │
│   │   ├── Production/                    # Manufacturing pages
│   │   │   ├── ProductionDashboard.razor
│   │   │   ├── DashboardFH.razor
│   │   │   ├── ProductionCalendar.razor
│   │   │   ├── WorkInProgress.razor
│   │   │   │
│   │   │   ├── NonConformance/            # Quality control
│   │   │   │   └── NCRReports.razor
│   │   │   │
│   │   │   ├── Setup/                     # Configuration
│   │   │   │   ├── Factory/FactoryLayout/
│   │   │   │   │   ├── FactoryLayout.razor
│   │   │   │   │   ├── DraggableZone.razor
│   │   │   │   │   └── ZoneDialog.razor
│   │   │   │   │
│   │   │   │   └── WorkCentres/
│   │   │   │       ├── WorkCentreManagementPg.razor
│   │   │   │       └── WorkCentreFunctionsPg.razor
│   │   │   │
│   │   │   └── Dialogs/
│   │   │       ├── WOPlanDialog.razor
│   │   │       └── PlanByWODrawer.razor
│   │   │
│   │   └── SearchPages/                   # Search interfaces
│   │       ├── ClientSearch.razor
│   │       └── CustomerSearch.razor
│   │
│   ├── UserSettings/                      # User preferences
│   │   ├── UserSettings.razor
│   │   └── UserMsgCentre.razor
│   │
│   ├── Notifications/                     # Messaging
│   │   ├── NewMessage.razor
│   │   └── SignalR.razor
│   │
│   └── aaBU/                              # Business unit pages
│       ├── TablesPgBU.razor
│       ├── RoleManagementPgBU.razor
│       └── WorkInProgress.razor
│
├── PulseComponents/                       # Reusable components
│   ├── Cards/
│   │   ├── PulseAICard.razor              # AI assistant card
│   │   └── PulseToast.razor               # Toast notification
│   │
│   ├── Containers/
│   │   ├── PulseToastContainer.razor      # Toast container
│   │   └── 3DViewer.razor                 # 3D visualization
│   │
│   └── Viewers/
│       └── Roller3DViewer.razor           # Three.js roller viewer
│
├── Pages/_Host.cshtml                     # HTML entry point
├── Pages/_Host.cshtml.cs                  # Code-behind
├── App.razor                              # Component root
└── Routes.razor                           # Route definitions
```

### Key Component Types

#### 1. **Layout Components**
Provide navigation structure and consistent UI shell across related pages:
- `MasterLayout.razor` - Primary application layout with menu, notifications, user panel
- Zone-specific layouts for Role-Based UI segregation

#### 2. **Page Components**
Full-screen pages with specific functionality:
- Interactive pages with forms, tables, charts
- Dashboard pages with analytics
- Dialog pages for detailed workflows

#### 3. **Reusable Components** (PulseComponents/)
- `PulseAICard` - Displays AI assistant information
- `Roller3DViewer` - Three.js-based 3D model viewer
- `PulseToastContainer` - Notification display system
- Custom input components for specialized data entry

---

## Service Layer Architecture

### Service Directory Structure

```
Services/
├── Authentication & Authorization
│   ├── AuthService.cs                     # JWT token & claims management
│   ├── CustomAuthenticationStateProvider.cs # Blazor auth provider
│   ├── CurrentUserService.cs              # Current user context
│   ├── TokenStorageService.cs             # Token persistence
│   └── TokenHolderService.cs              # Singleton token holder
│
├── API Communication
│   ├── PulseApiService.cs                 # Main API client (747 lines)
│   └── AuthHeaderHandler.cs               # HTTP message handler for JWT
│
├── AI Integration
│   ├── Pulse_AI.cs                        # Core AI orchestration (1179 lines)
│   ├── OllamaClient.cs                    # Ollama model client (388 lines)
│   ├── Global_AI_Functions.cs             # Shared AI utilities
│   └── AiPromptService.cs                 # AI prompt management
│
├── Real-Time Communication
│   └── MessageHub.cs                      # SignalR hub client (328 lines)
│
├── UI & Notifications
│   ├── PulseToastService.cs               # Toast notification service
│   └── IPulseToastService.cs              # Toast interface
│
├── State & Context
│   ├── CircuitIdService.cs                # Circuit identification
│   ├── CircuitState.cs                    # Circuit state tracking
│   └── GlobalFunctions.cs                 # Shared utilities
│
├── Specialized Services
│   ├── RollerModelGenerationService.cs    # 3D model generation
│   ├── MouseService.cs                    # Mouse event tracking
│   └── GlobalVars.cs                      # Global variables
│
└── Error Handling
	└── ApiErrorHandler.cs                 # Standardized error handling
```

### Core Service Implementations

#### 1. **AuthService** - Authentication Management

```csharp
public class AuthService
{
	// Token Management
	public string? JwtToken { get; private set; }

	// Claims Extraction
	public ClaimsPrincipal aspireUser { get; private set; }
	public string? aspireUserId { get; }
	public string? aspireUsername { get; }
	public string? aspireFullName { get; }
	public string? aspireEmail { get; }
	public IEnumerable<string> Roles { get; }
	public bool IsAuthenticated { get; }

	// Methods
	public async Task InitializeAsync()
	public async Task LoadTokenAsync()
	public async Task SetAuthenticatedAsync(string token)
	public async Task LogoutAsync()
}
```

**Responsibilities:**
- JWT token extraction and storage
- Claims parsing (user ID, roles, permissions)
- Token initialization on app startup
- Authentication state updates

**Integration Points:**
- `TokenStorageService` - Persistent token storage
- `TokenHolderService` - Singleton token holder
- `CustomAuthenticationStateProvider` - Blazor authentication provider

#### 2. **PulseApiService** - API Communication

**Large service (747 lines) providing:**

```csharp
public sealed class PulseApiService
{
	// HTTP Methods (Generic)
	public async Task<T?> GetAsync<T>(string requestUri, ...)
	public async Task<T?> PostAsync<T>(string requestUri, object payload, ...)
	public async Task<T?> PutAsync<T>(string requestUri, object payload, ...)
	public async Task<bool> DeleteAsync(string requestUri, ...)

	// Specialized Methods
	public async Task<List<Customer>> GetClientsByCompanyAsync(int companyId)
	public async Task<WorksOrder?> GetWorkOrderAsync(int woNo)
	public async IAsyncEnumerable<T> GetStreamAsync<T>(string requestUri, ...)

	// Bulk Operations
	public async Task<List<T>> GetMultipleAsync<T>(params string[] uris)
}
```

**Key Features:**
- Generic HTTP wrapper with JSON serialization
- Response deserialization to `ApiResponse<T>`
- Structured error handling
- Logging support
- CancellationToken support for cancellation patterns
- IAsyncEnumerable for streaming responses

**Configuration:**
- Base URL from appsettings (Aspire or fallback)
- Infinite timeout for long-running operations
- Custom `AuthHeaderHandler` for JWT injection

#### 3. **Pulse_AI** - AI Orchestration Service

**Large service (1179 lines) for AI intelligence:**

**Three Primary AI Assistants:**

1. **Ali (Analysis & Investigation)**
   - Document analysis
   - Contextual queries with `ContextualPrompt`
   - Analysis request queueing
   - Custom prompt templates

2. **Flapper (Conversational AI)**
   - Multi-turn conversations
   - Chat history management
   - Message streaming
   - Conversation persistence

3. **Tables (Structured Data AI)**
   - Database schema understanding
   - SQL generation
   - Query recommendations
   - Data exploration

**Core Capabilities:**

```csharp
public sealed class Pulse_AI
{
	// Schema & Caching
	private IMemoryCache cache;

	// AI Model Interaction
	public async IAsyncEnumerable<string> GenerateAsync(string prompt, ...)
	public async Task<T?> AnalyzeAsync<T>(string input, Type expectedType, ...)

	// Chat Operations (Flapper)
	public async IAsyncEnumerable<string> ChatAsync(FlapperConversation conversation, ...)
	public async Task<FlapperConversation> CreateConversationAsync(string userId)

	// Analysis Operations (Ali)
	public async Task<AnalysisRequest> QueueAnalysisAsync(AnalysisRequestDto request)
	public async Task<AliResult?> GetAnalysisResultAsync(int requestId)

	// Table Operations
	public async Task<List<dynamic>> ExecuteQueryAsync(string query, ...)
	public async Task<SchemaDto> GetDatabaseSchemaAsync()
}
```

**Key Features:**
- Streaming responses for real-time updates
- Schema caching for performance
- Tool-use support (SQL execution, web search)
- Error recovery and fallback strategies
- Integration with both local Ollama and cloud providers

#### 4. **OllamaService** - Local AI Model Client

**Purpose:** Communicate with Ollama local AI service

```csharp
public sealed class OllamaService
{
	// Connection Validation
	public async Task<bool> ValidateConnectionAsync(string modelName, ...)

	// Model Management
	public async Task<List<string>> ListModelsAsync()
	public async Task<bool> IsModelAvailableAsync(string modelName)

	// Generation
	public async IAsyncEnumerable<string> GenerateAsync(string prompt, ...)
	public async Task<string> EmbedAsync(string text, ...)

	// Configuration
	private Uri _localBaseAddress;    // Local Ollama endpoint
	private Uri _cloudBaseAddress;    // Cloud fallback
	private ConcurrentDictionary<string, bool> ModelCache;
}
```

**Configuration:**
- Endpoint: `OllamaApi:EndpointHttp` (default: http://192.168.0.6:11434)
- Supports both local and cloud Ollama instances
- Model caching for performance

#### 5. **MessageHub Service** - Real-Time Communication

**SignalR Hub Client** for real-time messaging:

```csharp
public class MessageHubService : IAsyncDisposable
{
	// Events for Real-Time Updates
	public event Action<PulseMessage>? OnReceiveMessage;
	public event Action<PulseMessage>? OnReceiveFlapperChunk;
	public event Action<List<PulseMessage>>? OnLoadHistory;
	public event Action<List<UserPresenceDto>>? OnPresenceListUpdated;

	// Connection Management
	public async Task EnsureConnectedAsync()
	public async Task SendMessageAsync(PulseMessage message)
	public async Task UpdatePresenceAsync(PresenceStatus status)

	// History
	public async Task LoadHistoryAsync(string userId)
}
```

**Features:**
- Automatic reconnection with exponential backoff
- JWT-based authentication
- User presence tracking
- Message history loading
- Notification streaming for Flapper responses

#### 6. **PulseToastService** - Notification System

```csharp
public class PulseToastService : IPulseToastService
{
	// Notification Methods
	public void ShowSuccess(string message, string aiEmoji = "PulseAI")
	public void ShowInformation(string message, string aiEmoji = "PulseAI")
	public void ShowWarning(string message, string aiEmoji = "PulseAI")
	public void ShowError(string message, string aiEmoji = "PulseAI")

	// Events
	public event Action<PulseToastMessage>? OnShow;
	public event Action<Guid>? OnRemove;
}
```

**AI Emoji Support:**
- Associates notifications with AI assistants
- PulseAI, Ali, Flapper, Tables personalities

---

## Authentication & Authorization

### JWT-Based Authentication Flow

```
1. User Login (Login.razor)
   ↓
2. POST /Security/login → PulseApiService
   ↓
3. Backend validates credentials → JWT token returned
   ↓
4. AuthService.SetAuthenticatedAsync(token)
   ├─ Parse JWT claims → aspireUser (ClaimsPrincipal)
   ├─ Extract roles and permissions
   └─ Store token → TokenStorageService
   ↓
5. CustomAuthenticationStateProvider.NotifyStateChanged()
   ↓
6. Components observe AuthenticationState
```

### Key Authentication Components

#### TokenStorageService
- Stores JWT in browser storage (SessionStorage/LocalStorage)
- Retrieves token on app initialization
- Persists authentication state across browser sessions

#### TokenHolderService
- Singleton service holding current token in memory
- Fast token access for HTTP header injection
- Cleared on logout

#### AuthHeaderHandler
- Custom HttpClientHandler
- Automatically injects JWT into Authorization header
- Handles token refresh scenarios

#### CustomAuthenticationStateProvider
- Implements `AuthenticationStateProvider`
- Converts AuthService.aspireUser to AuthenticationState
- Notifies Blazor of authentication changes

### Authorization Policies

```csharp
builder.Services.AddAuthorizationCore(options =>
{
	options.AddPolicy("Admin", policy =>
		policy.RequireRole("Admin"));

	options.AddPolicy("User", policy =>
		policy.RequireRole("User", "Admin"));
});
```

**Usage in Components:**
```razor
@attribute [Authorize(Roles = "Admin")]
@attribute [Authorize(Policy = "Admin")]
```

### Permission System Integration

Fine-grained permissions managed via:
- `Permission` entity (in Pulse.Models)
- `RolePermission` mapping
- Custom authorization handlers (can be extended)

---

## API Communication Layer

### PulseApiService Architecture

The `PulseApiService` follows several patterns:

#### 1. **Generic HTTP Wrapper**

```csharp
public async Task<T?> GetAsync<T>(string requestUri, CancellationToken ct = default) where T : class
{
	var response = await _httpClient.GetAsync(requestUri, ct);

	if (response.IsSuccessStatusCode)
	{
		return await response.Content.ReadAsAsync<T>(_jsonOptions);
	}

	// Error handling...
}
```

**Benefits:**
- Type-safe responses
- Reusable error handling
- Logging at single point
- CancellationToken propagation

#### 2. **ApiResponse<T> Pattern**

All responses wrapped in standardized `ApiResponse<T>` from Pulse.Models:

```csharp
// Component usage
var response = await _apiService.GetAsync<ApiResponse<Customer>>(
	ApiEndpoints.Clients.GetById(customerId)
);

if (response?.Success ?? false)
{
	var customer = response.Data; // Extract actual data
	// Use customer...
}
```

**Important:** Per `.github/copilot-instructions.md`, always deserialize to `ApiResponse<T>`, then extract the `Data` property.

#### 3. **Error Handling Pipeline**

```csharp
try
{
	// HTTP request
}
catch (HttpRequestException ex)
{
	_logger.LogError("HTTP error: {Error}", ex.Message);
	// User-friendly error message
}
catch (JsonException ex)
{
	_logger.LogError("JSON parsing error: {Error}", ex.Message);
	// Invalid response format
}
catch (OperationCanceledException)
{
	_logger.LogWarning("Request cancelled");
	// Timeout or cancellation
}
```

#### 4. **Streaming Responses**

For long-running operations:

```csharp
public async IAsyncEnumerable<T> GetStreamAsync<T>(
	string requestUri,
	CancellationToken ct = default) where T : class
{
	var response = await _httpClient.GetAsync(requestUri, 
		HttpCompletionOption.ResponseHeadersRead, ct);

	using var stream = await response.Content.ReadAsStreamAsync(ct);
	using var reader = new StreamReader(stream);

	while (!reader.EndOfStream)
	{
		var line = await reader.ReadLineAsync(ct);
		if (!string.IsNullOrEmpty(line))
		{
			yield return JsonSerializer.Deserialize<T>(line, _jsonOptions)!;
		}
	}
}
```

**Used for:**
- AI response streaming
- Real-time data updates
- Large file downloads

### HTTP Client Configuration

```csharp
// Ollama Client (Infinite timeout for AI inference)
builder.Services.AddHttpClient<OllamaService>("OllamaClient", client =>
{
	client.BaseAddress = new Uri(ollamaEndpoint);
	client.Timeout = Timeout.InfiniteTimeSpan;  // ← No timeout for long AI queries
})
.StopPollyTimeouts();

// Pulse API Client (With JWT auth handler)
builder.Services.AddHttpClient<PulseApiService>("PulseApiClient", client =>
{
	client.BaseAddress = new Uri(pulseApiEndpoint);
	client.Timeout = Timeout.InfiniteTimeSpan;
})
.StopPollyTimeouts()
.AddHttpMessageHandler<AuthHeaderHandler>();  // ← Injects JWT token
```

---

## AI Integration Services

### Three AI Assistant Architecture

```
┌─────────────────────────────────────────────┐
│         Pulse.Web Application               │
├─────────────────────────────────────────────┤
│  Pulse_AI Service (Orchestrator)            │
├──────────────┬──────────────┬───────────────┤
│              │              │               │
▼              ▼              ▼               
┌──────┐  ┌────────┐  ┌───────┐
│  Ali │  │Flapper │  │Tables │
│(Ana.)│  │(Chat)  │  │(Data) │
└──────┘  └────────┘  └───────┘
│              │              │
└──────────────┴──────────────┴───────────────┘
			   │
			   ▼
		┌────────────────┐
		│ Ollama Service │
		└────────────────┘
			   │
			   ▼
		┌────────────────┐
		│ Local/Cloud AI │
		│   (Ollama)     │
		└────────────────┘
```

### Ali - Analysis & Investigation Assistant

**Purpose:** Document analysis, data investigation, contextual queries

**Pages:**
- `Components/Pages/AiZone/AliPg.razor`

**Backend Support:**
- `AnalysisRequest` entity (queuing)
- `AliResult` entity (results)
- `ContextualPrompt` (prompt templates)
- `ContextualArea` (business contexts)

**Workflow:**
1. User uploads document or provides context
2. Ali analyzes using relevant prompts
3. Analysis queued in database
4. Results streamed back to UI
5. User can bookmark/save analysis

### Flapper - Conversational AI Assistant

**Purpose:** Multi-turn conversations, chat-based interactions

**Pages:**
- `Components/Pages/AiZone/FlapperPg.razor` (1131 lines)
- `Components/Pages/AiZone/FlapperSettings.razor`

**Backend Support:**
- `FlapperConversation` entity (chat sessions)
- `FlapperMessage` entity (individual messages)
- Role: "user", "assistant", "system"

**Workflow:**
1. User creates or loads conversation
2. Messages displayed with role-based styling
3. User input sent to AI
4. Response streamed with markdown rendering
5. Conversation history persisted
6. User presence updated in real-time

**Features:**
- Markdown rendering of AI responses
- Speech synthesis integration (Toolbelt.Blazor.SpeechSynthesis)
- Real-time streaming updates
- Conversation persistence
- Message role tracking (user/assistant)

### Tables - Structured Data AI Assistant

**Purpose:** Database schema understanding, SQL generation, data queries

**Pages:**
- `Components/Pages/AiZone/TablesPg.razor`

**Capabilities:**
- Schema introspection
- Natural language to SQL translation
- Query result visualization
- Data exploration assistance

**Workflow:**
1. AI loads database schema
2. User asks data question in natural language
3. AI generates SQL query
4. Query executed safely in sandbox
5. Results visualized as table/chart

---

## Real-Time Communication

### SignalR Integration

#### MessageHub Service
Provides client-side hub connection for real-time features:

```csharp
var hubConnection = new HubConnectionBuilder()
	.WithUrl(navigationManager.ToAbsoluteUri("/messagehub"))
	.WithAutomaticReconnect()
	.Build();
```

#### Features
- **User Presence Tracking** - Show who's online, status (Available, Busy, Away, etc.)
- **Real-Time Notifications** - Server-push notifications
- **Message Streaming** - Live message updates from AI
- **Broadcast Updates** - Global announcements

#### Event Subscription Pattern

```razor
@implements IAsyncDisposable

@code {
	protected override async Task OnInitializedAsync()
	{
		MsgHubService.OnReceiveMessage += HandleMessage;
		MsgHubService.OnPresenceListUpdated += HandlePresenceUpdate;
		await MsgHubService.EnsureConnectedAsync();
	}

	private void HandleMessage(PulseMessage message)
	{
		// Update component state
		StateHasChanged();
	}

	async ValueTask IAsyncDisposable.DisposeAsync()
	{
		MsgHubService.OnReceiveMessage -= HandleMessage;
	}
}
```

#### Backend Support (in Pulse.ApiService)
- SignalR hub accepting client connections
- Broadcasting presence updates
- Streaming responses to clients
- Message queuing and delivery

---

## State Management

### AppState - Cross-Component State

```csharp
public class AppState
{
	public event Func<Task>? OnChange;
	public event Func<Task>? OnThemeChange;

	public async Task NotifyStateChanged()
	public async Task NotifyThemeChanged()
}
```

**Usage Pattern:**

```razor
@inject AppState AppState

@code {
	protected override void OnInitialized()
	{
		AppState.OnChange += HandleStateChange;
	}

	private async Task HandleStateChange()
	{
		await InvokeAsync(StateHasChanged);
	}
}
```

**Observed States:**
- Theme changes
- Global notifications
- User session state
- Division/company context changes

### Circuit State - Blazor Server Circuits

```csharp
public interface ICircuitState
{
	string CircuitId { get; }
	Dictionary<string, object> State { get; }
}

public class CircuitState : ICircuitState { }

public class CircuitIdService : CircuitHandler
{
	public override async Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken ct)
	{
		// Initialize circuit state
	}
}
```

**Purposes:**
- Identify user session
- Track circuit lifecycle
- Cleanup on disconnect
- Rate limiting per circuit

### Token Holder Service

```csharp
public class TokenHolderService
{
	public string? CurrentToken { get; set; }
}
```

Singleton service for:
- Quick token access
- HTTP header injection
- Token refresh coordination

---

## UI Frameworks & Styling

### Component Libraries

#### 1. **Fluent UI** (Microsoft.FluentUI.AspNetCore.Components)
**Version:** 4.14.1

**Components Used:**
- FluentDesignTheme - Theming system
- FluentButton, FluentTextField - Form controls
- FluentDataGrid - Data tables
- FluentDialog - Modal dialogs
- FluentNavigation - Navigation menus

**Features:**
- Dark/Light theme support
- Accessibility-first
- Consistent design language
- Enterprise-grade polish

#### 2. **Bootstrap Blazor**
**Version:** 10.5.1

**Components:**
- BootstrapBlazor base components
- Chart components (BootstrapBlazor.Chart)
- FontAwesome icons (BootstrapBlazor.FontAwesome)
- Table components with sorting/filtering
- Form validation

#### 3. **Radzen Components**
**Version:** 10.3.1

**High-Level Components:**
- RadzenChart - Rich charting library
- RadzenGrid - Advanced data grid
- RadzenDropDown - Select components
- RadzenNotification - Toast notifications

#### 4. **FullCalendar Integration**
**External Library:** fullcalendar-scheduler@6.1.19

**Features:**
- Drag-and-drop scheduling
- Resource-based views
- Production timeline visualization
- JavaScript interop via `calendarInterop.js`

#### 5. **Three.js 3D Visualization**
**Files:**
- `three.core.min.js`, `three.module.min.js`
- `OrbitControls.js` - 3D camera controls
- `roller-viewer.js` - Roller-specific visualization
- `three-viewer.js` - Generic 3D viewer

**Components:**
- `Roller3DViewer.razor` - Blazor wrapper
- `3DRoller.razor` - Full-page 3D roller display

### CSS Framework & Styling

#### Bootstrap 5.3.7
- Responsive grid system
- Pre-built component styles
- Utility classes for layout

#### Custom CSS
- **Scoped Styling** - `.razor.css` files co-located with components
- **Global Styles** - `wwwroot/css/site.css`
- **Theme Variables** - CSS custom properties

**Example Scoped Styles:**
```css
/* MasterLayout.razor.css */
.drawer {
	--bb-drawer-body-padding: 0 !important;
	height: calc(100vh - 56px) !important;
	z-index: 999 !important;
}
```

### Responsive Design

- **Mobile-First** approach
- **Breakpoints** via Bootstrap classes
- **Flexbox & CSS Grid** for layouts
- **Viewport Meta Tags** for proper scaling

---

## Page Structure & Routing

### Routes Configuration

```razor
<!-- Routes.razor -->
<Router AppAssembly="typeof(Program).Assembly">
	<Found Context="routeData">
		<RouteView RouteData="routeData" DefaultLayout="typeof(MasterLayout)" />
		<FocusOnNavigate RouteData="routeData" Selector="h1" />
	</Found>
	<NotFound>
		<LayoutView Layout="typeof(MasterLayout)">
			<p role="alert">Sorry, there's nothing at this address.</p>
		</LayoutView>
	</NotFound>
</Router>
```

### Route Organization

#### Security Pages
- `/` - Home/Login
- `/security/login` - Login form
- `/security/login-to-division` - Division selection
- `/security/reset-password` - Password reset
- `/security/access-denied` - Unauthorized access

#### AI Zone (`/aizone/`)
- `/aizone/ali` - Analysis Assistant
- `/aizone/flapper` - Conversational AI
- `/aizone/tables` - Data Query AI
- `/aizone/flapper-settings` - AI Configuration

#### Employee Zone (`/employeezone/`)
- `/employeezone/` - Dashboard
- `/employeezone/customers/*` - CRM functions
- `/employeezone/production/*` - Manufacturing operations
- `/employeezone/search/*` - Search interfaces

#### Admin Zone (`/adminzone/`)
- `/adminzone/dashboard` - Admin overview
- `/adminzone/users` - User management
- `/adminzone/roles` - Role management
- `/adminzone/permissions` - Permission management
- `/adminzone/integration` - System integration
- `/adminzone/sensors` - Sensor management

#### User Settings (`/usersettings/`)
- `/usersettings/` - User preferences
- `/usersettings/messages` - Message center

### Layout Selection Strategy

Each page declares its layout via `@layout` directive:

```razor
@* Use MasterLayout for main app pages *@
@layout MasterLayout

@* Use NoLayout for full-page experiences *@
@layout NoLayout

@* Use zone-specific layouts for organization *@
@layout EmployeeZoneMainLayout
```

**Layout-to-Zone Mapping:**
- `MasterLayout` - Default for most pages
- `EmployeeZoneMainLayout` - Employee work area
- `EZ-AdminZoneLayout` - Admin area
- `ProductionLayout` - Production dashboards
- `NoLayout` - Full-screen: 3D viewer, login

---

## Configuration Management

### appsettings.json Structure

```json
{
  "Logging": {
	"LogLevel": {
	  "Default": "Information",
	  "Microsoft.AspNetCore": "Warning"
	}
  },
  "AllowedHosts": "*",
  "PulseApi": {
	"Endpoint": "http://localhost:7466",
	"EndpointHttps": "https://localhost:5475"
  },
  "OllamaApi": {
	"EndpointHttp": "http://192.168.0.6:11434/api/generate",
	"Key": "76c15b3643ba413aac7429bbb64e119a...",
	"WebSearchEndpoint": "https://ollama.com/api/web_search",
	"WebFetchEndpoint": "https://ollama.com/api/web_fetch"
  }
}
```

### Configuration Sources (in order of precedence)

1. **appsettings.Production.json** - Production overrides
2. **appsettings.Development.json** - Development overrides
3. **appsettings.json** - Base configuration
4. **User Secrets** (Development only) - Sensitive values
5. **Environment Variables** - Aspire/Docker overrides
6. **Aspire Configuration** - Service discovery

### Environment-Specific Files

**appsettings.Development.json:**
```json
{
  "Logging": {
	"LogLevel": {
	  "Default": "Debug",
	  "Microsoft": "Debug"
	}
  }
}
```

**appsettings.Production.json:**
```json
{
  "Logging": {
	"LogLevel": {
	  "Default": "Warning"
	}
  },
  "PulseApi": {
	"Endpoint": "https://api.pulse.prod/"
  }
}
```

### User Secrets (Development)

```bash
dotnet user-secrets set "OllamaApi:Key" "<secret-key>"
```

**Purpose:** Protect sensitive configuration during development

---

## Dependencies & NuGet Packages

### Authentication & Authorization
- **Microsoft.AspNetCore.Identity.EntityFrameworkCore** (10.0.7)
  - ASP.NET Core Identity integration
  - Role-based authorization

- **System.IdentityModel.Tokens.Jwt** (8.17.0)
  - JWT token parsing and validation
  - Claim extraction

- **Microsoft.AspNetCore.Authentication.JwtBearer** (10.0.7)
  - JWT authentication middleware (if API-mode enabled)

### Real-Time Communication
- **Microsoft.AspNetCore.SignalR.Client** (10.0.7)
  - SignalR hub client
  - Real-time message delivery
  - Automatic reconnection

### HTTP & API Communication
- **Microsoft.Extensions.Http.Polly** (10.0.7)
  - Polly resilience policies (retry, timeout)
  - Circuit breaker patterns

- **Microsoft.Extensions.ServiceDiscovery.Yarp** (10.5.0)
  - Dynamic service discovery
  - YARP reverse proxy integration
  - Aspire integration

### UI Component Libraries
- **Microsoft.FluentUI.AspNetCore.Components** (4.14.1)
  - Fluent Design System components
  - Enterprise UI toolkit

- **Microsoft.FluentUI.AspNetCore.Components.DataGrid.EntityFrameworkAdapter** (4.14.1)
  - EF Core integration for grids

- **Microsoft.FluentUI.AspNetCore.Components.Emoji** (4.14.1)
  - Emoji support for UI

- **Microsoft.FluentUI.AspNetCore.Components.Icons** (4.14.1)
  - Icon support

- **BootstrapBlazor** (10.5.1)
  - Bootstrap-based components
  - Extended form controls

- **BootstrapBlazor.Chart** (10.0.2)
  - Charting components

- **BootstrapBlazor.FontAwesome** (10.0.1)
  - FontAwesome icon support

- **Radzen.Blazor** (10.3.1)
  - Rich UI components
  - Advanced charts and grids

### Data Export & Processing
- **ClosedXML** (0.105.0)
  - Excel file generation and manipulation
  - Export to Excel functionality

- **Markdig** (1.1.3)
  - Markdown to HTML parsing
  - AI response rendering

### Speech & Audio
- **Toolbelt.Blazor.SpeechSynthesis** (11.0.0)
  - Text-to-speech in Blazor
  - Ali voice synthesis

- **Toolbelt.Blazor.SpeechRecognition** (1.0.0)
  - Voice input recognition
  - Natural language input

### Caching & State
- **Aspire.StackExchange.Redis.DistributedCaching** (13.2.4)
  - Distributed cache via Redis
  - Session state management

- **Aspire.StackExchange.Redis.OutputCaching** (13.2.4)
  - Output caching for Blazor pages
  - HTTP response caching

- **Blazor.SessionStorage** (9.0.1)
  - Browser session storage access
  - Client-side token persistence

### Observability
- **OpenTelemetry.Api** (1.15.3)
  - Distributed tracing support
  - Metrics and logging

- **OpenTelemetry.Exporter.OpenTelemetryProtocol** (1.15.3)
  - OTLP trace exporter
  - Integration with observability backends

### Resilience & Reliability
- **Polly** (8.6.6)
  - Fault-tolerance library
  - Retry, timeout, circuit breaker policies

### Animation & Effects
- **BlazorAnimation** (2.2.0)
  - CSS animation support in Blazor
  - Transition effects
  - Smooth UI interactions

### Project References
- **Pulse.Models** - Shared domain models
- **Pulse.ApiService** - Backend API client
- **Pulse.ServiceDefaults** - Cross-cutting defaults

---

## Design Patterns & Conventions

### 1. Razor Component Patterns

#### Page Components with Layout

```razor
@page "/aizone/flapper"
@layout MasterLayout
@rendermode InteractiveServer
@implements IAsyncDisposable

@code {
	protected override async Task OnInitializedAsync()
	{
		// Initialize component
	}

	async ValueTask IAsyncDisposable.DisposeAsync()
	{
		// Cleanup resources
	}
}
```

#### Cascading Parameters for State Passing

```razor
<CascadingValue Value="this">
	@ChildContent
</CascadingValue>

[CascadingParameter] public ParentComponent? Parent { get; set; }
```

#### Event Callbacks for Component Communication

```razor
<ChildComponent OnItemSelected="HandleItemSelected" />

private void HandleItemSelected(Item item)
{
	// Handle selection
}
```

### 2. Service Injection Patterns

#### Scoped Services (Per Circuit/Request)
- `AuthService` - User authentication
- `PulseApiService` - API communication
- `CircuitIdService` - Circuit identification
- Custom component services

#### Singleton Services
- `TokenHolderService` - Global token access
- `IPulseToastService` - Toast notifications
- `AppState` - Application state

#### Factory Pattern
- HttpClientFactory for specialized clients
- Service builders in Program.cs

### 3. Error Handling Patterns

#### Try-Catch with Logging

```csharp
try
{
	var result = await _apiService.GetAsync<ApiResponse<Customer>>(url);
	if (result?.Success ?? false)
	{
		return result.Data;
	}
}
catch (HttpRequestException ex)
{
	_logger.LogError("API call failed: {Error}", ex.Message);
	_toastService.ShowError("Failed to load data");
}
```

#### ApiResponse Pattern

```csharp
var response = await _apiService.PostAsync<ApiResponse<string>>(
	ApiEndpoints.Security.Login,
	loginModel
);

if (response?.Success ?? false)
{
	await _authService.SetAuthenticatedAsync(response.Data!);
}
else
{
	var errorMessage = response?.Message ?? "Login failed";
	_toastService.ShowError(errorMessage);
}
```

### 4. Async/Await Patterns

#### Streaming Responses

```razor
@foreach (var chunk in _chunks)
{
	<div>@chunk</div>
}

@code {
	private List<string> _chunks = new();

	private async Task StreamResponseAsync()
	{
		await foreach (var chunk in _apiService.GetStreamAsync<string>(url))
		{
			_chunks.Add(chunk);
			StateHasChanged();
		}
	}
}
```

#### Long-Running Operations

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(300));
try
{
	await _aiService.AnalyzeAsync(input, cts.Token);
}
catch (OperationCanceledException)
{
	_logger.LogWarning("Analysis timed out");
}
```

### 5. State Management Pattern

#### AppState for Global Changes

```csharp
// Notify all subscribers of state change
await _appState.NotifyStateChanged();

// Components subscribe:
protected override void OnInitialized()
{
	_appState.OnChange += HandleStateChange;
}
```

### 6. Authorization Patterns

#### Route-Level Authorization

```razor
@attribute [Authorize]
@attribute [Authorize(Roles = "Admin")]
@attribute [Authorize(Policy = "Admin")]
```

#### Runtime Authorization Checks

```csharp
if (!_authService.Roles.Contains("Admin"))
{
	_navigationManager.NavigateTo("/security/access-denied");
}
```

### 7. Naming Conventions

**Components:**
- PascalCase file names (FlapperPg.razor)
- Suffix with "Pg" for pages, "Dialog" for modals

**Services:**
- Suffix with "Service" (AuthService, PulseApiService)
- Prefix with interface "I" (IPulseToastService)

**Methods:**
- Async methods end with "Async"
- Event handlers prefix with "On" or "Handle"

**Properties:**
- PascalCase (CurrentUser, IsAuthenticated)
- Prefixed with underscore if private (_logger)

---

## Containerization

### Docker Support

**Dockerfile** (multi-stage build):

```dockerfile
# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["Pulse.Web/Pulse.Web.csproj", "Pulse.Web/"]
COPY ["Pulse.Models/Pulse.Models.csproj", "Pulse.Models/"]
COPY ["Pulse.ServiceDefaults/Pulse.ServiceDefaults.csproj", "Pulse.ServiceDefaults/"]
RUN dotnet restore "Pulse.Web/Pulse.Web.csproj"
COPY . .
RUN dotnet publish "Pulse.Web/Pulse.Web.csproj" -c Release -o /app/publish

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "Pulse.Web.dll"]
```

**Benefits:**
- Reproducible builds
- Minimal runtime image
- Multi-stage optimization
- Port 8080 for containerized environments

### Running in Docker

```bash
docker build -t pulse-web:latest .
docker run -p 8080:8080 \
  -e PulseApi__Endpoint=http://pulse-api:5000 \
  -e OllamaApi__EndpointHttp=http://ollama:11434 \
  pulse-web:latest
```

### Environment Variables

- `ASPNETCORE_URLS` - Server URLs
- `ASPNETCORE_ENVIRONMENT` - Environment (Development/Production)
- `PulseApi__Endpoint` - API service URL (double underscore for nested config)
- `OllamaApi__EndpointHttp` - Ollama endpoint

---

## Future Considerations

### Performance Optimizations

1. **Component Caching**
   - Implement `OnAfterRenderAsync` caching
   - Memoize expensive computations

2. **Virtual Scrolling**
   - For large lists (DataGrids, chat history)
   - Reduce DOM size for long conversations

3. **WebAssembly Offloading**
   - Move heavy computations to WASM
   - AI preprocessing on client

4. **Output Caching**
   - Cache HTML responses for static pages
   - Redis-backed cache via Aspire

### Scalability

1. **State Server**
   - Move from in-memory to Redis
   - Support multiple server instances

2. **Circuit Optimization**
   - Detect inactive circuits
   - Implement circuit pooling

3. **Load Balancing**
   - Deploy multiple Pulse.Web instances
   - Sticky sessions via cookie/URL

### Feature Enhancements

1. **Offline Support**
   - Service Worker for PWA features
   - Sync queue for offline actions

2. **Mobile App**
   - MAUI integration (Pulse.MobileApp project exists)
   - Responsive mobile layouts

3. **Advanced AI**
   - Multi-model support
   - Custom fine-tuned models
   - Model selection per operation

4. **Analytics**
   - User behavior tracking
   - Feature usage metrics
   - Performance monitoring

### Security Hardening

1. **CSRF Protection**
   - Verify anti-forgery tokens
   - SameSite cookie attributes

2. **Content Security Policy**
   - Restrict resource loading
   - Prevent XSS attacks

3. **Rate Limiting**
   - Per-user API rate limits
   - Circuit-based throttling

4. **Audit Logging**
   - Log sensitive operations
   - User action tracking

### Testing Strategy

1. **Unit Tests**
   - Test services in isolation
   - Mock HttpClient and dependencies

2. **Integration Tests**
   - Test component + service integration
   - Mock API responses

3. **E2E Tests**
   - Selenium/Playwright for full workflows
   - Test real browser scenarios

4. **Performance Tests**
   - Load testing with k6/JMeter
   - Benchmark critical paths

---

## Conclusion

**Pulse.Web** is a modern, feature-rich Blazor Server application that serves as the interactive frontend for a sophisticated manufacturing ERP system. Its architecture demonstrates:

- **Modern C# & Blazor patterns** for responsive, real-time UI
- **Multi-layered service architecture** for separation of concerns
- **Comprehensive AI integration** with three specialized assistants
- **Enterprise-grade UI components** from multiple frameworks
- **Production-ready security** with JWT authentication
- **Real-time capabilities** via SignalR for collaboration
- **Cloud-native design** with Docker support and Aspire integration

The codebase balances **complexity** (1179-line AI service, extensive component hierarchy) with **maintainability** (clear separation of concerns, consistent patterns) and **extensibility** (pluggable services, modular components).

---

**Document Version:** 1.0  
**Generated:** 2027-02-27  
**For:** Pulse.Web Project  
**Target Framework:** .NET 10.0  
**Application Type:** Blazor Server (Interactive)  
**Author:** Pulse Development Team
