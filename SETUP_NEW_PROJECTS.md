# Adding WinUI Desktop and MAUI Mobile Apps to Pulse.Aspire

## Overview
Two new projects have been successfully added to your Pulse solution:

1. **Pulse.DesktopApp** - WinUI 3 desktop application (Windows-only)
2. **Pulse.MobileApp** - .NET MAUI mobile application (Android-specific implementation)

## Project Structure

```
Pulse Aspire/
├── Pulse.AppHost/              (Aspire orchestration host)
├── Pulse.ApiService/           (Backend API)
├── Pulse.Web/                  (Blazor web app)
├── Pulse.DesktopApp/           (WinUI desktop - integrated in AppHost)
├── Pulse.MobileApp/            (MAUI mobile - runs independently)
├── Pulse.Models/               (Shared models)
└── Pulse.ServiceDefaults/      (Service configuration defaults)
```

## Desktop App (Pulse.DesktopApp - WinUI 3)

### Features
- **Platform**: Windows only
- **Framework**: WinUI 3 with XAML
- **Integration**: Registered in AppHost for orchestration
- **Location**: `Pulse.DesktopApp/`

### File Structure
```
Pulse.DesktopApp/
├── App.xaml              (Main application definition)
├── App.xaml.cs           (Application code-behind)
├── Assets/               (Images, icons, resources)
├── Views/                (XAML page layouts)
├── Properties/           (Project metadata)
└── Pulse.DesktopApp.csproj
```

### Getting Started
1. Navigate to `Pulse.DesktopApp/` in Visual Studio
2. Build the project: `dotnet build Pulse.DesktopApp`
3. Run directly or through AppHost orchestration

### API Connection
Configure the API endpoint in your App.xaml.cs or a configuration file:
```csharp
// Example: Connect to running PulseApi
var apiClient = new HttpClient 
{ 
    BaseAddress = new Uri("https://localhost:5000") // Update port as needed
};
```

---

## Mobile App (Pulse.MobileApp - .NET MAUI)

### Features
- **Platform**: Android (and optionally iOS, macOS, Windows if extended)
- **Framework**: .NET MAUI with C# and XAML
- **Task-Specific**: Only implements required Android-specific functionality, not full web app clone
- **Integration**: Runs independently from AppHost (MAUI is not compatible with Aspire orchestration)
- **Location**: `Pulse.MobileApp/`

### File Structure
```
Pulse.MobileApp/
├── App.xaml                    (Main application definition)
├── App.xaml.cs                 (Application logic)
├── AppShell.xaml               (Navigation shell)
├── MauiProgram.cs              (MAUI app configuration)
├── MainPage.xaml               (Home page)
├── MainPage.xaml.cs            (Page code-behind)
├── Platforms/                  (Platform-specific code)
│   ├── Android/                (Android-specific implementations)
│   ├── iOS/
│   ├── macOS/
│   └── Windows/
├── Resources/                  (Images, fonts, strings)
├── Services/                   (Business logic - API calls, data)
└── Pulse.MobileApp.csproj
```

### Building & Running

#### Build for Android
```powershell
# Debug build
dotnet build Pulse.MobileApp -f net10.0-android

# Release build
dotnet build Pulse.MobileApp -f net10.0-android -c Release
```

#### Deploy to Android Device/Emulator
```powershell
# Connect device or start emulator first
dotnet run -f net10.0-android
```

#### Run in Visual Studio
1. Open the Solution
2. Set `Pulse.MobileApp` as startup project
3. Select target Android device/emulator
4. Press F5 or click "Run Android"

### Android-Specific Tasks
The MAUI app should focus on:
- **Task-specific UI**: Only screens needed for mobile workflows
- **Native Android features**: Camera, location, notifications, etc.
- **Offline capability**: Caching and local storage
- **Connectivity handling**: Work with intermittent network

### API Configuration
Create a configuration service to connect to your PulseApi:

**Example: Services/ApiService.cs**
```csharp
public class ApiService
{
    private readonly HttpClient _httpClient;
    private const string ApiBaseUrl = "https://your-api-endpoint:5000"; // Change for production

    public ApiService()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(ApiBaseUrl);
    }

    public async Task<T> GetAsync<T>(string endpoint)
    {
        var response = await _httpClient.GetAsync(endpoint);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(json);
    }
}
```

Register in **MauiProgram.cs**:
```csharp
public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();
    builder
        .UseMauiApp<App>()
        .Services.AddSingleton<ApiService>();
    
    return builder.Build();
}
```

### Requirements for Android
- Android SDK (minimum: Android API 21, target: Latest)
- Java Development Kit (JDK 11+)
- Android Studio or equivalent emulator

---

## AppHost Configuration

The `Pulse.AppHost` orchestrates:
- ✅ Redis cache
- ✅ PulseApi service
- ✅ Pulse.Web (Blazor)
- ✅ Pulse.DesktopApp (WinUI)
- ❌ Pulse.MobileApp (runs independently - incompatible with Aspire)

### Running with Aspire
```powershell
# From solution root
dotnet run --project Pulse.AppHost
```

This starts:
- Aspire Dashboard at `https://localhost:18888`
- Redis Commander
- All orchestrated services

---

## Shared Code Between Projects

Both apps should reuse:
1. **Pulse.Models** - Data models and DTOs
2. **Pulse.ApiService** - API client/proxy (if needed)
3. **Common services** - Authentication, logging, configuration

### Adding Shared References
```csharp
// In Pulse.DesktopApp.csproj and Pulse.MobileApp.csproj
<ItemGroup>
    <ProjectReference Include="..\Pulse.Models\Pulse.Models.csproj" />
</ItemGroup>
```

---

## Development Workflow

### Option 1: Aspire Orchestration (Web + Desktop)
```powershell
# Terminal 1: Run AppHost
dotnet run --project Pulse.AppHost

# Terminal 2: Run Desktop App (optional - launches independently)
dotnet run --project Pulse.DesktopApp
```

### Option 2: Mobile Development (Standalone)
```powershell
# Run MAUI app on Android emulator
dotnet run --project Pulse.MobileApp -f net10.0-android
```

---

## Next Steps

1. **Desktop App Setup**
   - [ ] Define WinUI views and pages in `Views/`
   - [ ] Create ViewModels in a `ViewModels/` folder
   - [ ] Implement API communication
   - [ ] Add authentication/token management

2. **Mobile App Setup**
   - [ ] Define task-specific pages in root or `Pages/` folder
   - [ ] Create services in `Services/` folder
   - [ ] Implement Android-specific features
   - [ ] Add AppShell navigation structure
   - [ ] Configure API endpoint for mobile

3. **Shared Resources**
   - [ ] Move common models to `Pulse.Models`
   - [ ] Create shared API client library if needed
   - [ ] Setup authentication tokens/JWT handling

---

## Troubleshooting

### WinUI Project Issues
- Ensure you're on Windows 10/11 with latest SDK
- Check project file paths are correct
- Verify Assets, Views, and Properties folders exist

### MAUI Project Issues
- Install Android SDK through Visual Studio or manually
- Ensure `net10.0-android` target framework is available
- Check environment variables: `ANDROID_HOME`, `JAVA_HOME`
- Run `dotnet workload install android` if needed

### AppHost Build Errors
- Clean solution: `dotnet clean`
- Restore: `dotnet restore`
- Ensure project references point to correct relative paths
