# Pulse.Models Project Architecture Documentation

**Version:** 1.0  
**Last Updated:** 2027-02-27  
**Target Framework:** .NET 10.0  

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Project Overview](#project-overview)
3. [Architecture Overview](#architecture-overview)
4. [Domain Model Structure](#domain-model-structure)
5. [Database Context & Entity Framework](#database-context--entity-framework)
6. [API Infrastructure](#api-infrastructure)
7. [AI Integration Models](#ai-integration-models)
8. [Security & Identity Management](#security--identity-management)
9. [Domain Areas](#domain-areas)
10. [Communication Infrastructure](#communication-infrastructure)
11. [Custom Components & DTOs](#custom-components--dtos)
12. [Dependencies & NuGet Packages](#dependencies--nuget-packages)
13. [Design Patterns & Conventions](#design-patterns--conventions)
14. [Database Mapping Strategy](#database-mapping-strategy)
15. [Future Considerations](#future-considerations)

---

## Executive Summary

**Pulse.Models** is a comprehensive .NET 10 class library that serves as the central data layer and domain model for the Pulse application ecosystem. It implements a sophisticated multi-tenant manufacturing ERP system with advanced AI integration, focusing on industrial roller production, customer relationship management, and production planning.

### Key Characteristics:
- **Architecture:** Domain-Driven Design with Entity Framework Core
- **Purpose:** Shared domain models across Web, API, Desktop (MAUI), and Mobile applications
- **Industry Focus:** Manufacturing (industrial rollers), with emphasis on compounds, polymers, and production workflows
- **AI Integration:** Deep AI capabilities through multiple AI assistants (Ali, Flapper, Tables)
- **Security:** ASP.NET Core Identity with custom role-based permission system

---

## Project Overview

### Project Configuration

```xml
<TargetFramework>net10.0</TargetFramework>
<ImplicitUsings>enable</ImplicitUsings>
<Nullable>enable</Nullable>
```

### Project Type
- **SDK:** Microsoft.NET.Sdk
- **Output Type:** Class Library (DLL)
- **Nullable Reference Types:** Enabled throughout
- **Implicit Usings:** Enabled for modern C# development

### Primary Responsibilities
1. Define all domain entities and business objects
2. Provide Entity Framework Core database context
3. Establish API contracts and response wrappers
4. Support AI integration through specialized models
5. Manage user identity and permission structures
6. Facilitate cross-application data consistency

---

## Architecture Overview

### Architectural Style
**Pulse.Models** follows a **Clean Architecture** approach with clear separation of concerns:

```
┌─────────────────────────────────────────────────┐
│         Consuming Applications                  │
│   (Pulse.Web, Pulse.ApiService, MAUI Apps)      │
└─────────────────────────────────────────────────┘
					  │
					  ▼
┌─────────────────────────────────────────────────┐
│            Pulse.Models (This Project)          │
│  ┌──────────────────────────────────────────┐   │
│  │         API Infrastructure               │   │
│  │  (ApiResponse, ApiEndpoints, Exceptions) │   │
│  └──────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────┐   │
│  │         Domain Entities                  │   │
│  │  (Customers, Production, Compounds, etc.)│   │
│  └──────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────┐   │
│  │         AI Models                        │   │
│  │  (Ali, Flapper, Tables)                  │   │
│  └──────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────┐   │
│  │         Security & Identity              │   │
│  │  (ApplicationUser, Permissions, Roles)   │   │
│  └──────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────┐   │
│  │         PulseDbContext                   │   │
│  │  (Entity Configurations, DbSets)         │   │
│  └──────────────────────────────────────────┘   │
└─────────────────────────────────────────────────┘
					  │
					  ▼
┌─────────────────────────────────────────────────┐
│          SQL Server Database                    │
└─────────────────────────────────────────────────┘
```

### Layer Responsibilities

#### 1. **Domain Layer** (Core Business Objects)
Contains pure business entities representing the manufacturing domain:
- Customers and organizational structures
- Production work orders and planning
- Compounds, polymers, and roller specifications
- Geographic and industry classifications

#### 2. **Data Access Layer** (PulseDbContext)
Entity Framework Core implementation:
- DbContext configuration
- Entity mappings through Fluent API
- Database schema definitions
- Migration support

#### 3. **API Contract Layer** (Api namespace)
Standardized communication contracts:
- Generic response wrappers (`ApiResponse<T>`)
- Error handling structures
- Endpoint constants
- Custom exceptions

#### 4. **AI Integration Layer** (AI namespace)
Specialized models for AI features:
- Conversation management (Flapper)
- Analysis requests (Ali)
- Structured data queries (Tables)
- AI prompt management

#### 5. **Security Layer** (Users & Permissions)
Identity and authorization:
- ASP.NET Core Identity integration
- Custom permission system
- Role-based access control
- User settings and preferences

---

## Domain Model Structure

The domain models are organized into logical business areas:

### Directory Structure

```
Pulse.Models/
├── AI/                          # AI Assistant Models
│   ├── Ali/                     # Analysis & Investigation AI
│   ├── Flapper/                 # Conversational AI
│   └── Tables/                  # Structured Data AI
├── Api/                         # API Infrastructure
├── Communication/               # Messaging & Notifications
├── Compounds/                   # Material Science Domain
├── Customers/                   # Client Management
├── CustomComponents/            # Shared DTOs & Structures
├── Geographic/                  # Location Hierarchy
├── Industries/                  # Industry Classifications
├── Misc/                        # Utilities & Cross-cutting
├── Organizational/              # Company Structure
├── Permissions/                 # Authorization Models
├── Production/                  # Manufacturing Operations
│   ├── Layout/                  # Factory Floor Planning
│   ├── NonConformance/          # Quality Control
│   └── WorkTypes/               # Job Classifications
├── PulseContext/                # EF Core Context & Mappings
│   └── Maps/                    # Entity Configurations
├── Rollers/                     # Product Specifications
└── Users/                       # Identity & User Management
```

---

## Database Context & Entity Framework

### PulseDbContext Overview

The `PulseDbContext` is the central Entity Framework Core database context that inherits from `IdentityDbContext<ApplicationUser>`, providing:

1. **ASP.NET Core Identity Integration** for authentication/authorization
2. **DbSet Properties** for all domain entities
3. **Fluent API Configurations** through dedicated mapping classes

### Key Features

```csharp
public sealed class PulseDbContext : IdentityDbContext<ApplicationUser>
{
	// Constructor using primary constructor syntax (C# 12+)
	public PulseDbContext(DbContextOptions<PulseDbContext> options) 
		: base(options)
	{
	}

	// 50+ DbSet properties representing domain entities
	// Fluent API configurations in OnModelCreating
}
```

### DbSet Properties (Entity Collections)

The context exposes **50+ DbSet** properties organized by domain:

#### Customer Management
- `ClientMaster` - Customer entities
- `ClientContactMaster` - Customer contacts
- `ClientRollerMaster` - Customer roller inventory
- `ClientRollerSpecificationMaster` - Roller specifications
- `ClientSales` - Sales transactions
- `ClientBudgetMaster` - Budget tracking
- `ClientCalendarEvents` - Customer calendar

#### Organizational Structure
- `CompanyMaster` - Company entities
- `BranchMaster` - Branch locations
- `DivisionMaster` - Business divisions
- `RepresentativeMaster` - Sales representatives

#### Geographic Hierarchy
- `ContinentMaster` - Continents
- `CountryMaster` - Countries
- `ProvinceMaster` - Provinces/States
- `RegionMaster` - Regions

#### Production Management
- `WorksOrder` - Production work orders
- `ProductionPlanItems` - Production planning
- `WorkCentreMaster` - Work centers
- `WorkCentreFunctionsMaster` - Work center capabilities
- `ProductionStageMaster` - Production stages
- `WorkTypeMaster` - Work classifications
- `DivisionWorkTypes` - Division-specific work types
- `EquipmentItemMaster` - Equipment inventory
- `EquipmentCategoryMaster` - Equipment categories
- `EquipmentCapabilities` - Equipment capabilities
- `FactoryZones` - Factory layout zones
- `NonConformanceReports` - Quality issues

#### Materials & Compounds
- `CompoundMaster` - Compound formulations
- `CompoundRangeMaster` - Compound ranges
- `CompoundRangePropertyMaster` - Range properties
- `PolymerMaster` - Polymer types
- `HardnessTypeMaster` - Hardness classifications
- `ColourMaster` - Color specifications

#### Industry & Product Specifications
- `IndustryMaster` - Industry types
- `IndustryProcessMaster` - Industry processes
- `IndustryRollerEnvironmentMaster` - Operating environments
- `IndustryRecommendedCoverMaster` - Recommended covers
- `RollerTypeMaster` - Roller types
- `ShellTypeMaster` - Shell types

#### User & Security
- `UserMaster` - User profiles
- `UserSettingsMaster` - User preferences
- `AspNetUserSettings` - Application user settings
- `UserFavouriteQuery` - Saved user queries
- `UserFaveQueries` - Alternative saved queries
- `AspNetPermissions` - Permission definitions
- `AspNetRolePermissions` - Role-permission mappings
- `AspNetPermissionCategories` - Permission categories

#### AI & Communication
- `AiSavedQueries` - Saved AI queries
- `AnalysisRequests` - AI analysis requests (Ali)
- `FlapperConversations` - AI conversations (Flapper)
- `FlapperMessages` - Conversation messages
- `ContextualAreaMaster` - AI contextual areas
- `ContextualPromptMaster` - AI prompt templates
- `PulseMessages` - System messaging

#### General
- `PeriodMaster` - Fiscal/time periods

### Entity Configuration Strategy

All entity configurations are externalized into dedicated mapping classes following the **IEntityTypeConfiguration** pattern:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
	// Apply 50+ entity configurations
	modelBuilder.ApplyConfiguration(new ClientMasterMap());
	modelBuilder.ApplyConfiguration(new CompanyMasterMap());
	modelBuilder.ApplyConfiguration(new WorksOrderMap());
	// ... 47+ more configurations

	base.OnModelCreating(modelBuilder);
}
```

**Benefits:**
- Separation of concerns
- Maintainability through isolated configuration classes
- Clear table/column mappings
- Relationship definitions
- Index and constraint management

---

## API Infrastructure

The API infrastructure provides a standardized approach to API communication across all Pulse applications.

### ApiResponse<T> - Standardized Response Wrapper

The `ApiResponse<T>` class implements the **Result Pattern** for API communications:

```csharp
public class ApiResponse<T> where T : class
{
	public bool Success { get; set; }
	public T? Data { get; set; }
	public string? Message { get; set; }
	public Dictionary<string, string[]>? Errors { get; set; }
	public HttpStatusCode? StatusCode { get; set; }
	public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
```

#### Key Features:
1. **Success/Failure State** - Boolean indicator
2. **Typed Data Payload** - Generic data container
3. **User-Friendly Messages** - Contextual feedback
4. **Field-Level Validation Errors** - Structured error dictionary
5. **HTTP Status Code** - Standard status codes
6. **Audit Timestamp** - UTC timestamp for tracking
7. **Functional Programming Support** - Map, OnSuccess, OnFailure, Finally methods

#### Usage Patterns:

**Success Response:**
```csharp
return new ApiResponse<Customer>
{
	Success = true,
	Data = customer,
	Message = "Customer retrieved successfully",
	StatusCode = HttpStatusCode.OK
};
```

**Error Response:**
```csharp
return new ApiResponse<Customer>
{
	Success = false,
	Message = "Customer not found",
	StatusCode = HttpStatusCode.NotFound,
	Errors = new Dictionary<string, string[]>
	{
		["ClientID"] = new[] { "Customer with this ID does not exist" }
	}
};
```

**Blazor Client Pattern (per .github/copilot-instructions.md):**
```csharp
// Always deserialize to ApiResponse<T>, then extract Data
var response = await httpClient.GetFromJsonAsync<ApiResponse<Customer>>("/api/customers/123");
if (response.Success && response.Data != null)
{
	var customer = response.Data; // Extract the actual customer
}
```

### ApiEndpoints - Centralized Endpoint Constants

The `ApiEndpoints` static class provides centralized, type-safe API endpoint definitions:

```csharp
public static class ApiEndpoints
{
	// Organized by domain area
	public static class Security { ... }
	public static class PulseAi { ... }
	public static class Clients { ... }
	public static class Production { ... }
	// ... more domain areas
}
```

#### Endpoint Categories:

1. **Security**
   - Authentication (login, register, logout, refresh token)
   - User management (CRUD operations)
   - Role management
   - Permission management (get, create, update, assign, remove)
   - Permission categories

2. **PulseAI**
   - **Tables** - Structured data AI queries
   - **Flapper** - Conversational AI (chat sessions, conversation history)
   - **Ali** - Analysis AI (custom prompts, analysis queue)

3. **Additional Domains** (inferred from context, not fully shown in file):
   - Clients/Customers
   - Production management
   - Compounds
   - Rollers

### PulseApiException - Custom Exception

Domain-specific exception for API-related errors:

```csharp
public class PulseApiException : Exception
{
	public HttpStatusCode StatusCode { get; set; }
	public Dictionary<string, string[]>? Errors { get; set; }
}
```

### ApiErrorResponse

Standardized error response structure for consistent error handling.

---

## AI Integration Models

Pulse implements three distinct AI assistants, each with specialized purposes:

### 1. Ali - Analysis & Investigation Assistant

**Purpose:** Document analysis, data investigation, and contextual AI queries

#### Core Models:

**Ali.cs** - Configuration & Personality
```csharp
public class Ali
{
	public Emoji emoji { get; set; } // Detective emoji
	public SpeechSynthesisVoice? _Voice { get; set; }
	public string _VoiceId { get; set; } // "Microsoft Steffan Online"
	public string Model { get; set; } = "gpt-oss:latest";
}
```

**AnalysisRequest** - Persistent analysis tracking
- Stores AI analysis requests
- Queued processing support
- Request status tracking
- User association

**AliResult** - Analysis outcomes
- Analysis results
- Insights and recommendations

**AnalysisRequestDto** - Data transfer object for analysis requests

#### Features:
- Document analysis
- Custom prompt templates (`ContextualPrompt`)
- Contextual areas (`ContextualArea`)
- Speech synthesis integration via `Toolbelt.Blazor.SpeechSynthesis`
- Emoji personality via `Microsoft.FluentUI.AspNetCore.Components.Emoji`

### 2. Flapper - Conversational AI Assistant

**Purpose:** Natural language conversations, chat sessions, and interactive AI assistance

#### Core Models:

**FlapperConversation** - Conversation Management
```csharp
public class FlapperConversation
{
	public Guid Id { get; set; }
	public string UserId { get; set; }
	public string Title { get; set; } = "New Conversation";
	public DateTime StartedAt { get; set; }
	public DateTime LastActivity { get; set; }
	public string Status { get; set; } // Active | Completed

	public List<FlapperMessage> Messages { get; set; } = new();
}
```

**FlapperMessage** - Individual chat messages
```csharp
public class FlapperMessage
{
	public Guid Id { get; set; }
	public Guid ConversationId { get; set; }
	public string Role { get; set; } // "user" | "assistant" | "system"
	public string Content { get; set; }
	public DateTime Timestamp { get; set; }

	public FlapperConversation Conversation { get; set; }
}
```

**Flapper** - Configuration class

**FlapperChatStructures** - Supporting structures for chat functionality

#### Features:
- Multi-turn conversations with history
- User-specific conversation tracking
- Conversation status management
- Message threading and context preservation
- Ollama integration support

#### API Endpoints (from ApiEndpoints):
- `POST /PulseAI/Flapper/SendRequest` - Send chat message
- `POST /PulseAI/Flapper/ChatSession` - Manage chat session
- `POST /PulseAI/Flapper/OllamaChatSession` - Ollama-specific chat
- `GET /PulseAI/Flapper/Conversations/UserHistory/{userId}` - Get user conversations
- `GET /PulseAI/Flapper/Conversations/GetMessages/{conversationId}` - Get conversation messages

### 3. Tables - Structured Data AI Assistant

**Purpose:** Structured data queries and table-based AI analysis

#### Core Models:

**Tables.cs** - Configuration and query structures

#### Features:
- Structured data analysis
- Table-based query understanding
- Database schema interpretation

#### API Endpoints:
- `POST /PulseAI/Tables/SendRequest` - Send structured query

### Common AI Infrastructure

**PulseAiRequest** - Base AI request structure
**PulseAiResponse** - Base AI response structure
**SystemMessageData** - System-level AI message data
**AiQueryResponse** - Query-specific AI responses
**AiQuery** - Saved AI query entity

#### Contextual AI Support:
- `ContextualArea` - Define specific business areas for AI context
- `ContextualPrompt` - Reusable prompt templates for consistent AI behavior
- `UserFavouriteQuery` / `UserFavouriteQry` - User-saved queries for quick access

---

## Security & Identity Management

### ASP.NET Core Identity Integration

Pulse extends ASP.NET Core Identity with custom user properties and a sophisticated permission system.

### ApplicationUser - Extended Identity User

```csharp
public class ApplicationUser : IdentityUser
{
	public string FullName { get; set; }
	public bool RequirePwdChange { get; set; }
	public PresenceStatus PresenceStatus { get; set; } = PresenceStatus.Offline;
}

public enum PresenceStatus
{
	Unknown = -1,
	Busy = 0,
	OutOfOffice = 1,
	Away = 2,
	Available = 3,
	Offline = 4,
	DoNotDisturb = 5
}
```

**Extensions:**
- Full name storage
- Force password change flag
- Real-time presence tracking (for collaboration features)

### Custom Permission System

Pulse implements a granular, category-based permission system beyond basic role-based access:

#### Permission Entity
```csharp
public class Permission
{
	public int Id { get; set; }
	public string Name { get; set; }
	public string Description { get; set; }
	public string Category { get; set; } // "User Management", "Reports", "Settings"
	public DateTime CreatedDate { get; set; }

	public virtual PermissionCategory? CategoryNavigation { get; set; }
	public virtual ICollection<RolePermission> RolePermissions { get; set; }
}
```

#### RolePermission - Many-to-Many Mapping
```csharp
public class RolePermission
{
	public int Id { get; set; }
	public string RoleId { get; set; }
	public int PermissionId { get; set; }
	public DateTime AssignedDate { get; set; }

	public virtual Permission Permission { get; set; }
}
```

#### PermissionCategory - Logical Grouping
```csharp
public class PermissionCategory
{
	public int Id { get; set; }
	public string Name { get; set; }
	public string Description { get; set; }
	public int DisplayOrder { get; set; }

	public virtual ICollection<Permission> Permissions { get; set; }
}
```

### User-Related Models

**User** - Legacy/alternate user model
**UserDto** - Data transfer object for user operations
**UserSettings** - Per-user application settings
**ApplicationUserSettings** - Identity-integrated user settings

### Authentication Models

**LoginModel** - Login request
**RegisterModel** - Registration request
**ForgotPasswordModel** - Password reset initiation
**ResetPasswordModel** - Password reset completion
**AuthenticationToken** - JWT token response

### Database Seeding

**DatabaseSeeder** - Initial role setup:
```csharp
public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
{
	var roles = new[] { "Admin", "User" };
	// Create roles if they don't exist
}
```

---

## Domain Areas

### 1. Customer Management (Customers/)

#### Customer Entity (Aliased as ClientMaster)
Core customer information with comprehensive address management:

```csharp
public class Customer
{
	[Key]
	public required string FullClientID { get; set; }
	public required int CompanyID { get; set; }
	public required string ClientID { get; set; }
	public required string ClientName { get; set; }

	// Physical Address (5 lines)
	public string? Address1 { get; set; }
	// ... Address2 - Address5

	// Postal Address (3 lines)
	public string? PostalAddress1 { get; set; }
	// ... PostalAddress2 - PostalAddress3

	// Relationships
	public Company? Company { get; set; }
	public ICollection<ClientContact> ClientContacts { get; set; }
	public ICollection<ClientRoller> ClientRollers { get; set; }
	public ICollection<WorksOrder> WorksOrders { get; set; }
	// ... more relationships
}
```

#### Related Entities:
- **ClientContact** - Customer contact persons
- **ClientRoller** - Customer's roller inventory
- **ClientRollerSpecification** - Detailed roller specs
- **ClientSale** - Sales transactions
- **ClientBudgets** - Budget tracking and forecasting
- **ClientCalendarEvent** - Customer-related calendar events
- **ClientCurrentStats** - Current statistics (computed/cached)
- **CustomerUpdateDto** - Update operations DTO
- **ClientRollerSpecificationDto** - Specification transfer object

### 2. Organizational Structure (Organizational/)

Multi-level organizational hierarchy:

```
Company (Top Level)
  └── Branch (Location)
		└── Division (Business Unit)
			  └── SalesRepresentative (Person)
```

#### Entities:
- **Company** - Parent organization
- **Branch** - Physical locations
- **Division** - Business divisions/departments
- **SalesRepresentative** - Sales personnel

### 3. Geographic Hierarchy (Geographic/)

Four-level geographic classification:

```
Continent
  └── Country
		└── Province/State
			  └── Region
```

#### Entities:
- **Continent** - Continental classification
- **Country** - Country definitions
- **Province** - State/province level
- **Region** - Regional subdivisions

### 4. Production Management (Production/)

Comprehensive manufacturing workflow management:

#### WorksOrder - Core Production Entity
```csharp
public class WorksOrder
{
	public int WorksOrderNo { get; set; } // Primary Key
	public required string FullClientID { get; set; }
	public required int PeriodID { get; set; }
	public DateTime DateStarted { get; set; }
	public string? Description { get; set; }
	public required int WorkTypeID { get; set; }
	public required decimal Quantity { get; set; }
	public string DivisionID { get; set; }

	// Roller-specific fields
	public int? ClientRollerSpecificationID { get; set; }
	public int? ClientRollerID { get; set; }
	public string? ClientRollNo { get; set; }
	public string? CoverCompoundCode { get; set; }

	// Order references
	public string? ClientOrderNo { get; set; }
	public string? ClientPRNo { get; set; }
	public string? ClientRFQNo { get; set; }

	// Dimensions
	public decimal? ShellLength { get; set; }
	public decimal? ShellDiameter { get; set; }
	public decimal? CoverDiameter { get; set; }

	// Status tracking
	public int ProductionStageID { get; set; }
	public string? Status { get; set; }
	public DateTime? ProgressChange { get; set; }
	public string? ProgressComment { get; set; }

	// Relationships
	public Customer? Customer { get; set; }
	public Compound? Compound { get; set; }
	public WorkType? WorkType { get; set; }
	public ProductionStage? ProductionStage { get; set; }
	public ICollection<ProductionPlanItem> ProductionPlanItems { get; set; }
}
```

#### Supporting Entities:

**Production Planning:**
- **ProductionPlanItem** - Scheduled production items
- **ProductionStage** - Stage definitions (e.g., Mixing, Molding, Curing)
- **WorkType** - Job types
- **DivisionWorkType** - Division-specific work types

**Work Centers:**
- **WorkCentre** - Production work centers
- **WorkCentreFunctions** - Capabilities of work centers
- **EquipmentItem** - Equipment inventory
- **EquipmentCategory** - Equipment classifications
- **EquipmentCapability** - Equipment-workstation capabilities

**Quality Management:**
- **NonConformanceReport** - Quality issues and non-conformances

**Factory Layout (Production/Layout/):**
- **FactoryZone** - Physical factory zones
- **DomRect** - DOM rectangle for layout positioning (Blazor UI support)

**DTOs:**
- **WorkInProgressDto** - WIP reporting
- **WorkOrderUpdateDto** - Update operations

### 5. Compounds & Materials (Compounds/)

Material science domain for roller manufacturing:

#### Entities:
- **Compound** - Rubber/polymer compound formulations
- **CompoundRange** - Compound ranges/families
- **CompoundRangeProperty** - Properties of compound ranges
- **Polymer** - Base polymer types
- **HardnessType** - Hardness classifications (Shore A, Shore D, etc.)
- **Colour** - Color specifications

**Relationship:**
Polymer → CompoundRange → Compound → WorksOrder

### 6. Rollers & Products (Rollers/)

Product-specific models:

- **RollerType** - Types of rollers (e.g., idler, conveyor, guide)
- **ShellType** - Shell construction types
- **RollerModel3D** - 3D model data for visualization

### 7. Industries (Industries/)

Industry-specific classifications and recommendations:

- **Industry** - Industry types (Mining, Agriculture, Food Processing, etc.)
- **IndustryProcess** - Specific processes within industries
- **IndustryRollerEnvironment** - Operating environment conditions
- **IndustryRecommendedCover** - Recommended roller covers per industry

### 8. Miscellaneous (Misc/)

Cross-cutting domain models:

- **Period** - Fiscal/time periods
- **Colour** - Color specifications
- **EquipmentCategory** - Equipment categorization
- **AiQuery** - Saved AI queries
- **DatabaseSeeder** - Database initialization
- **ShowAiIcon** - UI helper for AI feature flags

---

## Communication Infrastructure

### PulseMessage - Internal Messaging System

Real-time messaging and notification system:

```csharp
public class PulseMessage
{
	public int Id { get; set; }

	// Participants
	public string? RecipientUserId { get; set; }
	public string? RecipientUserName { get; set; }
	public string? SenderUserId { get; set; }
	public string? SenderUserName { get; set; }

	// Message content
	public string? Subject { get; set; }
	public string Role { get; set; } = "user"; // "user", "pulseai", "stream", "notification"
	public string Content { get; set; }
	public string ContentType { get; set; } = "HTML"; // "HTML", "JSON", "MARKUP", "TEXT"

	// Tracking
	public DateTime? SentAt { get; set; }
	public bool? Delivered { get; set; }
}
```

**Use Cases:**
- User-to-user messaging
- AI-generated notifications
- System notifications
- Streaming updates

### Email Infrastructure

- **EmailConfiguration** - SMTP and email settings
- **EMailMessage** - Email message structure

---

## Custom Components & DTOs

### CustomComponents/ Directory

Shared structures and DTOs used across the application:

#### Calendar Integration
**CalendarStructures** - Calendar-related structures and DTOs

#### Ollama AI Integration
**OllamaStructures** - Structures for Ollama AI integration

#### User Presence
**UserPresenceDto** - Real-time user presence status transfer

#### PDF Generation
**PdfRequest** - PDF generation request structure

#### Database Schema
**SchemaDto** - Database schema representation for AI and tooling

#### Mapping Utilities
**MappingExtensions** - Extension methods for entity-to-DTO mapping

---

## Dependencies & NuGet Packages

### Package Analysis

```xml
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.0.7" />
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="10.0.7" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.7" />
<PackageReference Include="Microsoft.FluentUI.AspNetCore.Components.Emoji" Version="4.14.1" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.17.0" />
<PackageReference Include="Toolbelt.Blazor.SpeechSynthesis" Version="11.0.0" />
```

### Package Purposes:

#### 1. **Microsoft.AspNetCore.Identity.EntityFrameworkCore (10.0.7)**
- **Purpose:** ASP.NET Core Identity with EF Core storage
- **Used For:** User authentication, role management, identity persistence
- **Integration:** PulseDbContext inherits from IdentityDbContext<ApplicationUser>

#### 2. **Microsoft.EntityFrameworkCore (10.0.7)**
- **Purpose:** Object-Relational Mapping (ORM)
- **Used For:** Database access, LINQ queries, change tracking
- **Integration:** PulseDbContext and all entity mappings

#### 3. **Microsoft.EntityFrameworkCore.SqlServer (10.0.7)**
- **Purpose:** SQL Server database provider for EF Core
- **Used For:** SQL Server-specific features and optimizations
- **Target Database:** Microsoft SQL Server

#### 4. **Microsoft.FluentUI.AspNetCore.Components.Emoji (4.14.1)**
- **Purpose:** Fluent UI emoji support for Blazor
- **Used For:** AI personality (Ali's detective emoji)
- **Integration:** Ali.cs uses `Microsoft.FluentUI.AspNetCore.Components.Emojis`

#### 5. **System.IdentityModel.Tokens.Jwt (8.17.0)**
- **Purpose:** JWT token generation and validation
- **Used For:** API authentication, token-based security
- **Integration:** AuthenticationToken model, API authentication

#### 6. **Toolbelt.Blazor.SpeechSynthesis (11.0.0)**
- **Purpose:** Text-to-speech in Blazor applications
- **Used For:** AI voice synthesis (Ali's voice)
- **Integration:** Ali.cs uses `SpeechSynthesisVoice`

### Version Strategy
All Microsoft packages are aligned to **version 10.0.7** (latest .NET 10 stable releases), ensuring compatibility and stability.

---

## Design Patterns & Conventions

### 1. Domain-Driven Design (DDD)

**Ubiquitous Language:**
- Entities use business terminology (Customer, WorksOrder, Compound)
- Consistent naming across the domain

**Aggregates:**
- Customer aggregate root with ClientContact, ClientRoller
- WorksOrder aggregate root with ProductionPlanItem

**Value Objects:**
- Address structures (Address1-5, PostalAddress1-3)
- Geographic hierarchy (Continent → Country → Province → Region)

### 2. Repository Pattern (Implicit through EF Core)

DbSets act as repositories:
```csharp
context.ClientMaster.Where(c => c.CompanyID == companyId)
```

### 3. Result Pattern (API Layer)

`ApiResponse<T>` implements the Result pattern for API communication:
- Success/Failure encapsulation
- Error details
- Functional composition (Map, OnSuccess, OnFailure)

### 4. Data Transfer Objects (DTOs)

Separation of domain entities from API contracts:
- `CustomerUpdateDto` - For customer updates
- `ApplicationUserDto` - For user operations
- `PermissionDto` - For permission transfer
- `UserPresenceDto` - For presence status
- `SchemaDto` - For schema representation

### 5. Fluent API Configuration (EF Core)

Entity configurations externalized into dedicated classes:
```csharp
public class ClientMasterMap : IEntityTypeConfiguration<Customer>
{
	public void Configure(EntityTypeBuilder<Customer> builder)
	{
		builder.ToTable("ClientMaster");
		builder.HasKey(c => c.FullClientID);
		// ... additional configuration
	}
}
```

**Benefits:**
- Keeps entities clean (POCOs)
- Configuration reusability
- Better maintainability

### 6. Naming Conventions

**Entities:**
- PascalCase for class names (Customer, WorksOrder)
- PascalCase for properties (FullClientID, ClientName)

**DbSets:**
- Often suffixed with "Master" (ClientMaster, CompanyMaster)
- Reflects legacy database naming (common in migration scenarios)

**Keys:**
- Often prefixed with entity name (ClientID, CompanyID)
- Composite keys use "Full" prefix (FullClientID)

**Required Fields:**
- Marked with `[Required]` attribute or `required` keyword (C# 11+)

**Max Lengths:**
- Explicitly specified with `[MaxLength]` for string fields

### 7. Nullable Reference Types

Enabled throughout the project:
```xml
<Nullable>enable</Nullable>
```

Careful use of `?` for nullable reference types:
```csharp
public string? Address1 { get; set; } // Nullable
public required string ClientName { get; set; } // Required, non-null
```

### 8. Modern C# Features (C# 12+)

**Primary Constructors:**
```csharp
public sealed class PulseDbContext(DbContextOptions<PulseDbContext> options) 
	: IdentityDbContext<ApplicationUser>(options)
```

**Collection Expressions:**
```csharp
public ICollection<ProductionPlanItem> ProductionPlanItems { get; set; } = [];
```

**Required Members:**
```csharp
public required string FullClientID { get; set; }
```

---

## Database Mapping Strategy

### Explicit Table Mapping

All entities are mapped to specific database tables through fluent configurations:

```
Entity Class Name          →  Database Table Name
─────────────────────────────────────────────────
Customer                  →  ClientMaster
Company                   →  CompanyMaster
Branch                    →  BranchMaster
WorksOrder                →  WorksOrders
ApplicationUser           →  AspNetUsers (Identity)
Permission                →  AspNetPermissions
```

### Key Design Decisions

1. **Master Suffix Convention:**
   - Many tables use "Master" suffix (legacy naming)
   - Examples: ClientMaster, CompanyMaster, PolymerMaster

2. **Composite Keys:**
   - `FullClientID` pattern for multi-tenant scenarios
   - Combines CompanyID + ClientID for uniqueness

3. **Foreign Key Relationships:**
   - Explicit navigation properties
   - Both sides of relationships defined where appropriate

4. **Cascade Behavior:**
   - Configured per relationship in mapping classes
   - Prevents orphaned records

5. **Indexes:**
   - Performance-critical fields indexed
   - Composite indexes for common queries

### Migration Strategy

While migrations aren't visible in the project files, the structure suggests:
- Code-First approach with explicit configurations
- LatestSqlDb/270226.zip suggests database backup/snapshot
- Likely using EF Core migrations for schema evolution

---

## Future Considerations

### Scalability

1. **Multi-Tenancy:**
   - Current structure supports multi-tenancy through CompanyID
   - Consider tenant isolation strategies (database-per-tenant vs. shared database)

2. **Performance:**
   - Consider read/write segregation (CQRS)
   - Implement caching layer (Redis) for frequently accessed data
   - Optimize N+1 query scenarios with Include/ThenInclude

3. **AI Integration:**
   - Current AI models are tightly coupled
   - Consider abstracting AI provider interfaces for pluggability
   - Implement AI response caching

### Maintainability

1. **Versioning:**
   - Consider API versioning strategy for ApiResponse<T>
   - Version compatibility handling for mobile/desktop apps

2. **Testing:**
   - Current project appears to lack dedicated test models/mocks
   - Consider adding test data builders/factories

3. **Documentation:**
   - Generate XML documentation comments for public APIs
   - Consider generating API documentation from endpoint constants

### Security

1. **Permissions:**
   - Current permission system is comprehensive
   - Consider implementing permission caching
   - Add permission audit logging

2. **Data Protection:**
   - Consider implementing field-level encryption for sensitive data
   - Add audit trails for critical entities (Customer, WorksOrder)

### Technology Evolution

1. **.NET Updates:**
   - Keep aligned with .NET releases (currently .NET 10)
   - Monitor breaking changes in EF Core updates

2. **AI Technologies:**
   - Abstract AI providers for easier switching
   - Consider adding AI model versioning

3. **Blazor Integration:**
   - Current structure well-suited for Blazor
   - Consider Blazor United patterns for enhanced interactivity

---

## Conclusion

**Pulse.Models** is a mature, well-structured domain model library serving as the foundation for a comprehensive manufacturing ERP system with advanced AI integration. Its architecture demonstrates:

- **Clean separation of concerns** across domain areas
- **Modern C# practices** leveraging .NET 10 features
- **Comprehensive domain coverage** for industrial roller manufacturing
- **Strong AI integration** with multiple specialized assistants
- **Robust security model** with granular permissions
- **API-first design** with standardized response patterns

The project successfully balances legacy naming conventions (Master tables) with modern architectural patterns, providing a stable foundation for a multi-platform application ecosystem (Web, API, Desktop, Mobile).

---

**Document Version:** 1.0  
**Generated:** 2027-02-27  
**For:** Pulse.Models Project  
**Target Framework:** .NET 10.0  
**Author:** Pulse Development Team
