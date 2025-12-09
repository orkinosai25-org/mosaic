# Migration from Oqtane to OrkinosaiCMS

## Overview

This document outlines the design decisions and architectural differences between Oqtane v10 and OrkinosaiCMS, helping understand the evolution and improvements made in this implementation.

## Key Architectural Differences

### 1. Project Structure

**Oqtane v10:**
```
Oqtane.Server/
Oqtane.Client/
Oqtane.Shared/
Oqtane.Framework/
```

**OrkinosaiCMS:**
```
OrkinosaiCMS.Core/           # Domain entities and interfaces
OrkinosaiCMS.Infrastructure/ # Data access and services
OrkinosaiCMS.Web/            # Blazor Web App
OrkinosaiCMS.Modules.*/      # Individual modules
OrkinosaiCMS.Shared/         # Shared DTOs
```

**Rationale:** Clean Architecture principles with clear separation of concerns, making the codebase more maintainable and testable.

### 2. Module System

**Oqtane Approach:**
- Interface-based module system (`IModule`, `IModuleControl`)
- Convention-based discovery
- Separate client and server assemblies

**OrkinosaiCMS Approach:**
- Attribute-based discovery using `[Module]` attribute
- Single unified Blazor component model
- Settings dictionary for flexible configuration
- Automatic registration via reflection

**Example:**

```csharp
// Oqtane
public class MyModule : IModule
{
    public ModuleDefinition Definition { get; set; }
}

// OrkinosaiCMS
[Module("MyModule", "My Module", Category = "Content")]
public class MyModule : ModuleBase
{
    public override string ModuleName => "MyModule";
    // Implementation
}
```

**Rationale:** Simplified module creation with less boilerplate, while maintaining flexibility.

### 3. Page Model

**Oqtane:**
- Page templates
- Theme-based rendering
- Container abstraction

**OrkinosaiCMS:**
- SharePoint-inspired Master Pages
- Content zones defined in JSON
- Hierarchical page structure
- Application Pages concept

**Rationale:** More familiar to SharePoint users, with clearer separation between layout (Master Page) and content (Application Page).

### 4. Permission System

**Oqtane:**
- Role-based permissions
- Entity-level permissions
- Permission inheritance

**OrkinosaiCMS:**
- SharePoint-inspired permission levels
- Fine-grained permissions (View, Edit, Delete, Manage, etc.)
- Role-Permission matrix
- User-Role assignments

**Permission Mapping:**

| SharePoint Level | Oqtane Equivalent | OrkinosaiCMS Role |
|-----------------|-------------------|-------------------|
| Full Control | Administrator | Administrator |
| Design | Manager | Designer |
| Edit | Editor | Editor |
| Contribute | Contributor | Contributor |
| Read | Reader | Reader |

**Rationale:** More granular control over permissions, familiar to enterprise users from SharePoint.

### 5. Database Schema

**Key Improvements:**
- Soft delete pattern across all entities
- Audit fields (CreatedOn, CreatedBy, ModifiedOn, ModifiedBy) on base entity
- Entity configurations using Fluent API
- Automatic timestamp management
- Query filters for soft deletes

**Migration Path:**
```sql
-- Example: Sites table comparison

-- Oqtane
CREATE TABLE Site (
    SiteId INT PRIMARY KEY,
    Name NVARCHAR(200),
    -- Other fields
)

-- OrkinosaiCMS
CREATE TABLE Sites (
    Id INT PRIMARY KEY,
    Name NVARCHAR(200),
    CreatedOn DATETIME2 NOT NULL,
    CreatedBy NVARCHAR(256),
    ModifiedOn DATETIME2,
    ModifiedBy NVARCHAR(256),
    IsDeleted BIT NOT NULL DEFAULT 0,
    -- Other fields
)
```

### 6. Theme System

**Oqtane:**
- Theme packages with layouts and containers
- Theme-specific resources
- CSS isolation

**OrkinosaiCMS:**
- Theme entity in database
- Asset path configuration
- Master Page independence
- Theme settings as JSON

**Rationale:** Separation of structure (Master Pages) from styling (Themes) for greater flexibility.

### 7. Dependency Injection

**Oqtane:**
- Custom service registration
- Scoped services per tenant

**OrkinosaiCMS:**
- Standard ASP.NET Core DI
- Service interfaces in Core
- Implementations in Infrastructure
- Easy to mock for testing

### 8. API Design

**Oqtane:**
- RESTful controllers in Server project
- Separate client-side services

**OrkinosaiCMS:**
- Unified Blazor Web App model
- Service layer for business logic
- Direct component-to-service communication
- Optional API endpoints for external integration

## Feature Comparison

| Feature | Oqtane v10 | OrkinosaiCMS | Notes |
|---------|-----------|--------------|-------|
| Multi-tenancy | ✅ Built-in | 🔄 Planned | Architecture supports it |
| Module System | ✅ Advanced | ✅ Simplified | Easier to create modules |
| Themes | ✅ Rich | ✅ Flexible | Separate from layouts |
| Page Management | ✅ Templates | ✅ Master Pages | SharePoint-inspired |
| Permissions | ✅ Role-based | ✅ Fine-grained | SharePoint-inspired |
| Localization | ✅ Built-in | 🔄 Planned | Coming soon |
| Workflow | ✅ Basic | 🔄 Planned | Content approval |
| Versioning | ✅ Available | 🔄 Planned | Page versioning |
| Search | ✅ Built-in | 🔄 Planned | Full-text search |
| File Management | ✅ Advanced | 🔄 Planned | Asset management |

## Migration Steps

### For Site Administrators

1. **Export Content**: Export pages and content from Oqtane
2. **Map Structure**: Map Oqtane pages to OrkinosaiCMS pages
3. **Create Master Pages**: Design equivalent layouts
4. **Import Content**: Use migration tools (to be developed)
5. **Configure Permissions**: Set up roles and permissions
6. **Test Thoroughly**: Verify all functionality

### For Module Developers

1. **Refactor to Attribute-Based**: Add `[Module]` attributes
2. **Inherit from ModuleBase**: Use provided base class
3. **Update Settings**: Convert to dictionary-based settings
4. **Remove Server/Client Split**: Unified Blazor component
5. **Test Module**: Ensure functionality in new framework

### For Theme Developers

1. **Separate Layout from Theme**: Create Master Pages for structure
2. **Extract CSS**: Theme package for styling only
3. **Configure Asset Paths**: Set up theme assets
4. **Test Rendering**: Verify across different Master Pages

## Code Migration Examples

### Module Migration

**Oqtane Module:**
```csharp
public class BlogModule : IModule, IModuleControl
{
    public ModuleDefinition Definition { get; set; }
    
    [Parameter]
    public ModuleInstance ModuleInstance { get; set; }
    
    protected override void OnInitialized()
    {
        // Load blog posts
    }
}
```

**OrkinosaiCMS Module:**
```csharp
[Module("Blog", "Blog Module", Category = "Content")]
public class BlogModule : ModuleBase
{
    public override string ModuleName => "Blog";
    public override string Title => "Blog";
    public override string Description => "Display blog posts";
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        // Load blog posts
    }
}
```

### Service Migration

**Oqtane Service:**
```csharp
public class BlogService : ServiceBase, IBlogService
{
    public BlogService(HttpClient http) : base(http) { }
    
    public async Task<List<Post>> GetPostsAsync(int moduleId)
    {
        return await http.GetJsonAsync<List<Post>>($"api/blog/{moduleId}");
    }
}
```

**OrkinosaiCMS Service:**
```csharp
public class BlogService : IBlogService
{
    private readonly ApplicationDbContext _context;
    
    public BlogService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<Post>> GetPostsAsync(int moduleId)
    {
        return await _context.Posts
            .Where(p => p.ModuleId == moduleId)
            .ToListAsync();
    }
}
```

## Best Practices for Migration

1. **Start Small**: Migrate one module at a time
2. **Test Extensively**: Unit and integration tests
3. **Preserve Data**: Use soft deletes, never hard delete
4. **Document Changes**: Keep migration notes
5. **Version Control**: Commit frequently during migration
6. **Backup First**: Always backup before migration

## Breaking Changes

1. **Module Interface**: Different base class and attributes
2. **API Calls**: Direct service injection instead of HTTP calls
3. **Settings Storage**: Dictionary-based instead of strongly-typed
4. **Page Structure**: Master Pages instead of templates
5. **Permission Model**: Different permission structure

## Advantages of OrkinosaiCMS

1. **Cleaner Architecture**: Better separation of concerns
2. **Easier Testing**: Mockable interfaces and services
3. **Simpler Modules**: Less boilerplate code
4. **SharePoint Familiarity**: Familiar to enterprise users
5. **Modern Patterns**: Latest .NET features and patterns
6. **Better Performance**: Optimized queries and caching strategies
7. **Flexible Extensibility**: Multiple extension points

## Limitations

1. **No Built-in Multi-tenancy**: Planned for future releases
2. **Limited Migration Tools**: Manual migration required initially
3. **Smaller Module Ecosystem**: New platform, fewer ready-made modules
4. **No Built-in Localization**: Planned for future releases

## Future Roadmap

1. **Q1 2026**: Multi-tenancy support
2. **Q2 2026**: Migration tools from Oqtane
3. **Q3 2026**: Workflow engine
4. **Q4 2026**: Advanced search and versioning

## Conclusion

OrkinosaiCMS takes the proven concepts from Oqtane and refines them with Clean Architecture principles and SharePoint-inspired patterns, creating a more maintainable and enterprise-ready CMS platform. While migration requires effort, the long-term benefits in maintainability, testability, and extensibility make it worthwhile.

## Resources

- [OrkinosaiCMS Architecture](ARCHITECTURE.md)
- [Setup Guide](SETUP.md)
- [Extensibility Guide](EXTENSIBILITY.md)
- [Oqtane Documentation](https://docs.oqtane.org/)
