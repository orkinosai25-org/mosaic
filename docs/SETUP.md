# OrkinosaiCMS Setup Guide

## Prerequisites

### Required Software

- **.NET 10 SDK**: [Download](https://dotnet.microsoft.com/download/dotnet/10.0)
- **Visual Studio 2022 (17.12+)** or **Visual Studio 2026**: For IDE development
- **SQL Server 2019+** or **SQL Server Express**: For database
  - Alternative: **SQLite** for development
- **Git**: For version control
- **Node.js 20+** (optional): For frontend build tools

### Recommended Tools

- **SQL Server Management Studio (SSMS)**: Database management
- **Azure Data Studio**: Cross-platform database tool
- **Postman**: API testing
- **Docker Desktop**: For containerization

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/orkinosai25-org/orkinosaiCMS.git
cd orkinosaiCMS
```

### 2. Restore Dependencies

```bash
dotnet restore OrkinosaiCMS.sln
```

### 3. Configure Database

The database is already configured with Entity Framework Core 10 and includes:
- Complete entity models for all CMS features
- Initial database migration ready to apply
- Repository pattern implementation
- Unit of Work for transaction management

#### Option A: LocalDB (Recommended for Windows Development)

LocalDB is already configured in `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=OrkinosaiCMS;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
  }
}
```

No additional configuration needed for LocalDB on Windows.

#### Option B: SQL Server (Production or Non-Windows Development)

1. Create a database:
```sql
CREATE DATABASE OrkinosaiCMS;
```

2. Update connection string in `src/OrkinosaiCMS.Web/appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=OrkinosaiCMS;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
  }
}
```

For SQL Server authentication:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=OrkinosaiCMS;User Id=sa;Password=YourPassword;TrustServerCertificate=True;MultipleActiveResultSets=True"
  }
}
```

#### Option C: Azure SQL Database (Production)

See [Azure Deployment Guide](AZURE_DEPLOYMENT.md) for complete Azure setup instructions.

Quick configuration for Azure SQL:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:{your-server}.database.windows.net,1433;Initial Catalog=OrkinosaiCMS;User ID={username};Password={password};Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
}
```

### 4. Apply Database Migrations

The initial migration is already created. Apply it to create the database schema:

#### Install EF Core Tools (First Time Only)

```bash
dotnet tool install --global dotnet-ef --version 10.0.0
```

#### Apply Migrations

```bash
cd src/OrkinosaiCMS.Infrastructure
dotnet ef database update --startup-project ../OrkinosaiCMS.Web
```

This will create all tables:
- Sites, Pages, MasterPages
- Modules, PageModules
- Users, Roles, Permissions
- UserRoles, RolePermissions
- Themes

#### Verify Database Creation

```bash
# List all migrations
dotnet ef migrations list --startup-project ../OrkinosaiCMS.Web

# Generate SQL script (optional)
dotnet ef migrations script --startup-project ../OrkinosaiCMS.Web --output schema.sql
```

### 5. Build the Solution

```bash
cd ../../
dotnet build OrkinosaiCMS.sln
```

### 6. Run the Application

```bash
cd src/OrkinosaiCMS.Web
dotnet run
```

The application will be available at:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`

## Visual Studio Setup

### 1. Open Solution

1. Launch Visual Studio 2022/2026
2. Open `OrkinosaiCMS.sln`
3. Wait for solution to load and restore NuGet packages

### 2. Configure Startup Project

1. Right-click on `OrkinosaiCMS.Web` in Solution Explorer
2. Select "Set as Startup Project"

### 3. Configure Multiple Startup Projects (Optional)

For advanced scenarios with multiple services:
1. Right-click on solution → Properties
2. Select "Multiple startup projects"
3. Set `OrkinosaiCMS.Web` to "Start"

### 4. Run with Debugging

Press `F5` or click "Start Debugging"

### 5. Run without Debugging

Press `Ctrl+F5` or click "Start Without Debugging"

## Development Environment Configuration

### appsettings.Development.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=OrkinosaiCMS;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "CMS": {
    "SiteName": "OrkinosaiCMS Development",
    "DefaultTheme": "Default",
    "EnableModuleDiscovery": true,
    "AllowRegistration": true
  }
}
```

### appsettings.json (Production)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "CMS": {
    "SiteName": "OrkinosaiCMS",
    "DefaultTheme": "Default",
    "EnableModuleDiscovery": false,
    "AllowRegistration": false
  }
}
```

## Database Management

### Creating Migrations

When you modify entity models:

```bash
cd src/OrkinosaiCMS.Infrastructure
dotnet ef migrations add <MigrationName> --startup-project ../OrkinosaiCMS.Web
```

### Applying Migrations

```bash
dotnet ef database update --startup-project ../OrkinosaiCMS.Web
```

### Reverting Migration

```bash
dotnet ef database update <PreviousMigrationName> --startup-project ../OrkinosaiCMS.Web
dotnet ef migrations remove --startup-project ../OrkinosaiCMS.Web
```

### Generating SQL Scripts

```bash
dotnet ef migrations script --startup-project ../OrkinosaiCMS.Web --output migration.sql
```

## Creating Your First Module

### 1. Create Module Project

```bash
dotnet new razorclasslib -n OrkinosaiCMS.Modules.MyModule -o src/Modules/OrkinosaiCMS.Modules.MyModule
cd src/Modules/OrkinosaiCMS.Modules.MyModule
dotnet add reference ../../OrkinosaiCMS.Modules.Abstractions
cd ../../../
dotnet sln add src/Modules/OrkinosaiCMS.Modules.MyModule
```

### 2. Create Module Component

Create `MyModule.razor`:

```razor
@using OrkinosaiCMS.Modules.Abstractions
@inherits ModuleBase

<div class="my-module">
    <h3>@Title</h3>
    <p>@GetSetting("Message", "Hello from MyModule!")</p>
</div>

@code {
    public override string ModuleName => "MyModule";
    public override string Title => "My Custom Module";
    public override string Description => "A sample custom module";
    public override string Category => "Custom";
    
    private string GetSetting(string key, string defaultValue)
    {
        if (Settings != null && Settings.TryGetValue(key, out var value))
        {
            return value?.ToString() ?? defaultValue;
        }
        return defaultValue;
    }
}
```

### 3. Add Module Attribute

Update the component with the attribute:

```csharp
[Module("MyModule", "My Custom Module", Description = "A sample custom module", Category = "Custom")]
public class MyModule : ModuleBase
{
    // ... implementation
}
```

### 4. Reference in Web Project

```bash
cd src/OrkinosaiCMS.Web
dotnet add reference ../Modules/OrkinosaiCMS.Modules.MyModule
```

### 5. Register Module

The module will be automatically discovered on application startup.

## Testing

### Unit Tests

Create a test project:

```bash
dotnet new xunit -n OrkinosaiCMS.Tests -o tests/OrkinosaiCMS.Tests
cd tests/OrkinosaiCMS.Tests
dotnet add reference ../../src/OrkinosaiCMS.Core
dotnet add reference ../../src/OrkinosaiCMS.Infrastructure
cd ../../
dotnet sln add tests/OrkinosaiCMS.Tests
```

Run tests:

```bash
dotnet test
```

### Integration Tests

```bash
dotnet new xunit -n OrkinosaiCMS.IntegrationTests -o tests/OrkinosaiCMS.IntegrationTests
```

## Debugging

### Visual Studio Debugging

1. Set breakpoints by clicking in the left margin
2. Press F5 to start debugging
3. Use F10 (Step Over) and F11 (Step Into)
4. View variables in Locals, Autos, Watch windows

### Browser DevTools

1. Open browser DevTools (F12)
2. Use Console for JavaScript debugging
3. Network tab for HTTP requests
4. Application tab for storage inspection

### EF Core Logging

Enable detailed logging in `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

## Docker Setup

### Dockerfile

Create `Dockerfile` in root:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/OrkinosaiCMS.Web/OrkinosaiCMS.Web.csproj", "OrkinosaiCMS.Web/"]
COPY ["src/OrkinosaiCMS.Core/OrkinosaiCMS.Core.csproj", "OrkinosaiCMS.Core/"]
COPY ["src/OrkinosaiCMS.Infrastructure/OrkinosaiCMS.Infrastructure.csproj", "OrkinosaiCMS.Infrastructure/"]
RUN dotnet restore "OrkinosaiCMS.Web/OrkinosaiCMS.Web.csproj"
COPY src/ .
WORKDIR "/src/OrkinosaiCMS.Web"
RUN dotnet build "OrkinosaiCMS.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OrkinosaiCMS.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrkinosaiCMS.Web.dll"]
```

### Build and Run

```bash
docker build -t orkinosaicms .
docker run -p 8080:80 orkinosaicms
```

### Docker Compose

Create `docker-compose.yml`:

```yaml
version: '3.8'

services:
  web:
    build: .
    ports:
      - "8080:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Server=db;Database=OrkinosaiCMS;User=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True
    depends_on:
      - db

  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Passw0rd
    ports:
      - "1433:1433"
    volumes:
      - sqldata:/var/opt/mssql

volumes:
  sqldata:
```

Run with Docker Compose:

```bash
docker-compose up
```

## Troubleshooting

### Common Issues

#### 1. Migration Errors

**Problem**: "No DbContext was found"

**Solution**: Ensure startup project is set correctly:
```bash
dotnet ef migrations add InitialCreate --startup-project ../OrkinosaiCMS.Web --project .
```

#### 2. Connection Errors

**Problem**: Cannot connect to database

**Solution**: 
- Verify SQL Server is running
- Check connection string
- Ensure firewall allows connections
- Test connection with SSMS

#### 3. Build Errors

**Problem**: Project reference issues

**Solution**: Clean and rebuild:
```bash
dotnet clean
dotnet restore
dotnet build
```

#### 4. Module Not Found

**Problem**: Module doesn't appear after creation

**Solution**:
- Ensure module has `[Module]` attribute
- Reference module project in Web project
- Restart application to trigger discovery

### Getting Help

- **GitHub Issues**: [Report bugs](https://github.com/orkinosai25-org/orkinosaiCMS/issues)
- **Documentation**: Check docs folder
- **Stack Overflow**: Tag with `orkinosaicms`

## Next Steps

1. Read [Architecture Documentation](ARCHITECTURE.md)
2. Review [Extensibility Guide](EXTENSIBILITY.md)
3. Check [Migration Guide](MIGRATION.md) if coming from Oqtane
4. Start building your first module
5. Join the community discussions

## Additional Resources

- [.NET 10 Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Blazor Documentation](https://docs.microsoft.com/en-us/aspnet/core/blazor/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [Bootstrap 5](https://getbootstrap.com/docs/5.0/)
