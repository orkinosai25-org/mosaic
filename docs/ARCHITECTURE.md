# Mosaic Conversational CMS Architecture

> **⚠️ Note**: This document describes the **Mosaic Conversational CMS** proprietary SaaS platform architecture. This is NOT the future open-source "OrkinosaiCMS" project.

## Overview

Mosaic Conversational CMS is a modern, modular Content Management System built on .NET 10 and Blazor. It combines best practices of modern web development with proven enterprise CMS patterns, enhanced for multi-tenant SaaS deployment.

## Design Principles

1. **Modularity First**: Everything is a module - content, navigation, user management
2. **Clean Architecture**: Clear separation between Core, Infrastructure, and UI layers
3. **SharePoint-Inspired**: Familiar concepts like Master Pages, Web Parts (Modules), and permission levels
4. **Modern Stack**: Built on .NET 10, Blazor, and Entity Framework Core
5. **Extensibility**: Plugin architecture allowing third-party extensions
6. **SaaS-Ready**: Multi-tenant architecture for cloud deployment

## Architecture Layers

### 1. Core Layer (`OrkinosaiCMS.Core`)

The core layer contains the domain entities and business logic interfaces.

**Key Components:**
- **Domain Entities**: Site, Page, MasterPage, Module, Theme, User, Role, Permission
- **Interfaces**: Repository and service contracts
- **Common Classes**: Base entity with audit fields and soft delete support

**Design Decisions:**
- All entities inherit from `BaseEntity` for consistent auditing
- Soft delete pattern for data preservation
- Navigation properties for efficient querying

### 2. Infrastructure Layer (`OrkinosaiCMS.Infrastructure`)

Implements data access, external services, and cross-cutting concerns.

**Key Components:**
- **ApplicationDbContext**: EF Core DbContext with entity configurations
- **Repositories**: Data access implementations
- **Services**: Business logic implementations (e.g., ModuleService)
- **Configurations**: Fluent API entity configurations

**Design Decisions:**
- Repository pattern for data access abstraction
- Entity configurations separate from entities
- Automatic timestamp management in SaveChangesAsync
- Support for SQL Server (extensible to other databases)

### 3. Modules Abstractions (`OrkinosaiCMS.Modules.Abstractions`)

Defines the contract for creating CMS modules.

**Key Components:**
- **IModule**: Base interface for all modules
- **ModuleBase**: Abstract Blazor component for modules
- **ModuleAttribute**: Decorator for module discovery

**Design Decisions:**
- Attribute-based module discovery
- Settings dictionary for flexible configuration
- Lifecycle hooks for initialization

### 4. Shared Layer (`OrkinosaiCMS.Shared`)

Contains DTOs and shared models for client-server communication.

### 5. Web Layer (`OrkinosaiCMS.Web`)

Blazor Web App hosting the CMS UI.

**Key Components:**
- **Program.cs**: Application startup and service registration
- **Components**: Blazor components and pages
- **Layouts**: Master page implementations

## Module System

### Module Architecture

The module system is inspired by Oqtane's modular architecture and SharePoint's Web Parts:

1. **Module Definition**: Registered in the `Modules` table
2. **Module Instance**: `PageModule` links a module to a page with specific settings
3. **Module Discovery**: Automatic scanning of assemblies for `[Module]` attributes
4. **Module Rendering**: Dynamic component rendering based on type information

### Creating a Module

```csharp
[Module("MyModule", "My Custom Module", Category = "Custom")]
public class MyCustomModule : ModuleBase
{
    public override string ModuleName => "MyModule";
    public override string Title => "My Custom Module";
    public override string Description => "A custom module example";
    
    // Your module logic here
}
```

## Page System (SharePoint-Inspired)

### Master Pages

Master Pages define the layout structure with content zones:

```json
{
  "ContentZones": ["Header", "Main", "Sidebar", "Footer"]
}
```

- Reusable across multiple pages
- Define placeholders for modules
- Theme-independent layout structure

### Application Pages

Application Pages are content pages that:
- Use a Master Page for layout
- Contain modules in specific zones
- Support hierarchical navigation
- Have permission-based access control

## Permission System (SharePoint-Inspired)

### Permission Model

- **Permissions**: Fine-grained actions (View, Edit, Delete, Manage, etc.)
- **Roles**: Collections of permissions (Administrator, Editor, Contributor, Reader)
- **User Roles**: Many-to-many relationship between users and roles

### Permission Levels

Similar to SharePoint permission levels:
- **Full Control**: All permissions
- **Design**: Manage site design and structure
- **Edit**: Create, edit, and delete content
- **Contribute**: Add and edit own content
- **Read**: View-only access

## Database Schema

### Core Tables

1. **Sites**: Site collections
2. **Pages**: Individual pages with hierarchy
3. **MasterPages**: Layout templates
4. **Modules**: Module definitions
5. **PageModules**: Module instances on pages
6. **Themes**: Visual themes
7. **Users**: User accounts
8. **Roles**: Permission roles
9. **Permissions**: Individual permissions
10. **UserRoles**: User-role assignments
11. **RolePermissions**: Role-permission assignments

### Key Relationships

- Site → Pages (1:N)
- Page → PageModules (1:N)
- Module → PageModules (1:N)
- User → UserRoles → Role → RolePermissions → Permission

## Technology Stack

### Backend
- **.NET 10**: Latest .NET framework
- **C# 13**: Modern C# features
- **Entity Framework Core 10**: ORM for database access
- **SQL Server**: Primary database (extensible)

### Frontend
- **Blazor**: Component-based UI framework
- **Bootstrap 5**: CSS framework
- **JavaScript Interop**: For advanced UI features

### Development Tools
- **Visual Studio 2022/2026**: Primary IDE
- **dotnet CLI**: Command-line tooling
- **Entity Framework Tools**: Migration management

## Deployment Architecture

### Development
- Local SQL Server or SQLite
- IIS Express or Kestrel
- File-based configuration

### Production
- Azure App Service or Docker containers
- Azure SQL Database
- Azure Key Vault for secrets
- Application Insights for monitoring

## Extensibility Points

1. **Custom Modules**: Create Razor Class Libraries with modules
2. **Custom Themes**: Implement theme packages
3. **Custom Master Pages**: Design new layouts
4. **Service Injection**: Replace default services
5. **Event Hooks**: Subscribe to CMS events (planned)

## Security Considerations

1. **Authentication**: ASP.NET Core Identity integration
2. **Authorization**: Role-based and permission-based
3. **Input Validation**: Data annotations and FluentValidation
4. **SQL Injection**: Parameterized queries via EF Core
5. **XSS Protection**: Blazor automatic encoding
6. **CSRF Protection**: Built into Blazor forms

## Performance Optimization

1. **Query Filters**: Automatic soft delete filtering
2. **Eager Loading**: Include related entities when needed
3. **Caching**: Planned for frequently accessed data
4. **Lazy Loading**: On-demand component loading
5. **Database Indexing**: On URLs, paths, and foreign keys

## Future Enhancements

1. **Multi-tenancy**: Full SaaS support
2. **Workflow Engine**: Content approval workflows
3. **Version Control**: Page and content versioning
4. **Search Integration**: Full-text search with Lucene.NET or Azure Search
5. **CDN Integration**: Static asset delivery
6. **Localization**: Multi-language support
7. **API Layer**: RESTful API for headless CMS scenarios
8. **Real-time Features**: SignalR for collaborative editing

## Comparison with Oqtane

| Feature | OrkinosaiCMS | Oqtane |
|---------|-------------|--------|
| Architecture | Clean Architecture | Modular Monolith |
| Page Model | SharePoint-inspired Master Pages | Templates |
| Modules | Attribute-based discovery | Interface-based |
| Permissions | Fine-grained SharePoint-style | Role-based |
| Database | EF Core with configurations | EF Core with conventions |
| Extensibility | Plugin architecture | Module framework |

## Conclusion

OrkinosaiCMS combines the proven patterns from SharePoint with modern .NET technologies and Oqtane's modular architecture, creating a flexible, extensible, and enterprise-ready CMS platform.
