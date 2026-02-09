# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution files
COPY ["Pulse.ApiService/Pulse.ApiService.csproj", "Pulse.ApiService/"]
COPY ["Pulse.Models/Pulse.Models.csproj", "Pulse.Models/"]
COPY ["Pulse.ServiceDefaults/Pulse.ServiceDefaults.csproj", "Pulse.ServiceDefaults/"]

# Restore dependencies
RUN dotnet restore "Pulse.ApiService/Pulse.ApiService.csproj"

# Copy all source code
COPY . .

# Publish the application
RUN dotnet publish "Pulse.ApiService/Pulse.ApiService.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8081
ENV ASPNETCORE_URLS=http://+:8081

ENTRYPOINT ["dotnet", "Pulse.ApiService.dll"]
