# OrkinosaiCMS Database Architecture

This document describes the database architecture, entity models, and data access patterns used in OrkinosaiCMS.

## Overview

OrkinosaiCMS uses **Entity Framework Core 10.0** with **SQL Server** (including LocalDB and Azure SQL) as the primary database provider. The architecture follows clean architecture principles with:

- **Domain Entities** in `OrkinosaiCMS.Core`
- **DbContext and Migrations** in `OrkinosaiCMS.Infrastructure`
- **Repository Pattern** for data access abstraction
- **Unit of Work** for transaction management

## Database Schema

### Core Tables

#### Sites
Represents a site in the CMS (similar to SharePoint Site Collection).

| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key |
| Name | nvarchar(200) | Site name |
| Description | nvarchar(max) | Site description |
| Url | nvarchar(500) | Site URL (unique) |
| ThemeId | int | Associated theme |
| LogoUrl | nvarchar(max) | Logo URL |
| FaviconUrl | nvarchar(max) | Favicon URL |
| AdminEmail | nvarchar(256) | Administrator email |
| IsActive | bit | Active status |
| DefaultLanguage | nvarchar(10) | Default culture |
| CreatedOn | datetime2 | Creation timestamp |
| CreatedBy | nvarchar(max) | Creator |
| ModifiedOn | datetime2 | Last modified timestamp |
| ModifiedBy | nvarchar(max) | Last modifier |
| IsDeleted | bit | Soft delete flag |

#### Pages
Represents individual pages in the CMS.

| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key |
| SiteId | int | Foreign key to Sites |
| ParentId | int | Parent page (self-reference) |
| Title | nvarchar(200) | Page title |
| Path | nvarchar(500) | URL path (unique per site) |
| Content | nvarchar(max) | Page content |
| MasterPageId | int | Layout template |
| Order | int | Display order |
| IsPublished | bit | Published status |
| ShowInNavigation | bit | Show in navigation |
| MetaDescription | nvarchar(max) | SEO meta description |
| MetaKeywords | nvarchar(max) | SEO keywords |
| IconCssClass | nvarchar(max) | Icon CSS class |
| RequiredPermission | nvarchar(max) | Required permission |
| CreatedOn, CreatedBy, ModifiedOn, ModifiedBy, IsDeleted | | Audit fields |

#### MasterPages
Layout templates for pages (similar to SharePoint Master Pages).

| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key |
| SiteId | int | Foreign key to Sites |
| Name | nvarchar(200) | Template name |
| Description | nvarchar(max) | Description |
| ComponentPath | nvarchar(500) | Razor component path |
| ThumbnailUrl | nvarchar(max) | Preview image |
| IsDefault | bit | Default template flag |
| ContentZones | nvarchar(max) | Available content zones (JSON) |
| CreatedOn, CreatedBy, ModifiedOn, ModifiedBy, IsDeleted | | Audit fields |

#### Modules
Module definitions (similar to SharePoint Web Parts).

| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key |
| Name | nvarchar(200) | Unique module name |
| Title | nvarchar(200) | Display title |
| Description | nvarchar(max) | Module description |
| Category | nvarchar(100) | Category |
| Version | nvarchar(50) | Version |
| AssemblyName | nvarchar(500) | Assembly name |
| ComponentType | nvarchar(500) | Component type name |
| IconCssClass | nvarchar(max) | Icon CSS class |
| IsEnabled | bit | Enabled status |
| IsSystem | bit | System module flag |
| DefaultSettings | nvarchar(max) | Default settings (JSON) |
| RequiredPermissions | nvarchar(max) | Required permissions |
| CreatedOn, CreatedBy, ModifiedOn, ModifiedBy, IsDeleted | | Audit fields |

#### PageModules
Module instances on specific pages.

| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key |
| PageId | int | Foreign key to Pages |
| ModuleId | int | Foreign key to Modules |
| Title | nvarchar(200) | Instance title |
| Zone | nvarchar(50) | Content zone |
| Order | int | Order within zone |
| ShowTitle | bit | Show title flag |
| Settings | nvarchar(max) | Instance settings (JSON) |
| ContainerCssClass | nvarchar(max) | CSS class |
| IsVisible | bit | Visibility flag |
| CreatedOn, CreatedBy, ModifiedOn, ModifiedBy, IsDeleted | | Audit fields |

#### Themes
Visual themes for the CMS.

| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key |
| Name | nvarchar(200) | Theme name |
| Description | nvarchar(max) | Description |
| Version | nvarchar(50) | Version |
| Author | nvarchar(max) | Author name |
| AssetsPath | nvarchar(500) | Assets path |
| ThumbnailUrl | nvarchar(max) | Preview image |
| IsEnabled | bit | Enabled status |
| IsSystem | bit | System theme flag |
| DefaultSettings | nvarchar(max) | Default settings (JSON) |
| CreatedOn, CreatedBy, ModifiedOn, ModifiedBy, IsDeleted | | Audit fields |

### Security Tables

#### Users
User accounts in the system.

| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key |
| Username | nvarchar(100) | Unique username |
| Email | nvarchar(256) | Unique email |
| DisplayName | nvarchar(200) | Display name |
| PasswordHash | nvarchar(500) | Hashed password |
| IsActive | bit | Active status |
| EmailConfirmed | bit | Email verification status |
| LastLoginOn | datetime2 | Last login timestamp |
| AvatarUrl | nvarchar(max) | Avatar image URL |
| CreatedOn, CreatedBy, ModifiedOn, ModifiedBy, IsDeleted | | Audit fields |

#### Roles
Security roles (similar to SharePoint Permission Levels).

| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key |
| Name | nvarchar(100) | Unique role name |
| Description | nvarchar(max) | Description |
| IsSystem | bit | System role flag |
| CreatedOn, CreatedBy, ModifiedOn, ModifiedBy, IsDeleted | | Audit fields |

#### Permissions
Fine-grained permissions (similar to SharePoint).

| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key |
| Name | nvarchar(100) | Unique permission key |
| DisplayName | nvarchar(200) | Display name |
| Description | nvarchar(max) | Description |
| Category | nvarchar(100) | Category |
| IsSystem | bit | System permission flag |
| CreatedOn, CreatedBy, ModifiedOn, ModifiedBy, IsDeleted | | Audit fields |

#### UserRoles
Many-to-many relationship between Users and Roles.

| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key |
| UserId | int | Foreign key to Users |
| RoleId | int | Foreign key to Roles |
| CreatedOn, CreatedBy, ModifiedOn, ModifiedBy, IsDeleted | | Audit fields |

**Unique Index**: (UserId, RoleId)

#### RolePermissions
Many-to-many relationship between Roles and Permissions.

| Column | Type | Description |
|--------|------|-------------|
| Id | int | Primary key |
| RoleId | int | Foreign key to Roles |
| PermissionId | int | Foreign key to Permissions |
| CreatedOn, CreatedBy, ModifiedOn, ModifiedBy, IsDeleted | | Audit fields |

**Unique Index**: (RoleId, PermissionId)

## Relationships

### Site Hierarchy
```
Site (1) ──> (*) Pages
Site (1) ──> (*) MasterPages
```

### Page Structure
```
Page (1) ──> (*) Page (Children)
Page (*) ──> (1) MasterPage
Page (1) ──> (*) PageModules
```

### Module System
```
Module (1) ──> (*) PageModules
PageModules (*) ──> (1) Page
```

### Security Model
```
User (1) ──> (*) UserRoles (*) ──> (1) Role
Role (1) ──> (*) RolePermissions (*) ──> (1) Permission
```

## Indexes

### Performance Indexes
- `Sites.Url` (unique)
- `Pages.SiteId, Pages.Path` (unique composite)
- `PageModules.PageId, PageModules.Zone, PageModules.Order` (composite)
- `Modules.Name` (unique)
- `MasterPages.SiteId, MasterPages.Name` (unique composite)
- `Users.Username` (unique)
- `Users.Email` (unique)
- `Roles.Name` (unique)
- `Permissions.Name` (unique)
- `UserRoles.UserId, UserRoles.RoleId` (unique composite)
- `RolePermissions.RoleId, RolePermissions.PermissionId` (unique composite)

## Data Access Patterns

### Repository Pattern

The application uses a generic repository pattern for data access:

```csharp
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task AddRangeAsync(IEnumerable<T> entities);
    void Update(T entity);
    void UpdateRange(IEnumerable<T> entities);
    void Remove(T entity);  // Soft delete
    void RemoveRange(IEnumerable<T> entities);  // Soft delete
    void HardDelete(T entity);  // Physical delete
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
}
```

**Usage Example:**
```csharp
// Inject repository
private readonly IRepository<Site> _siteRepository;
private readonly IUnitOfWork _unitOfWork;

// Query
var activeSites = await _siteRepository.FindAsync(s => s.IsActive);

// Add
var site = new Site { Name = "My Site", Url = "/mysite" };
await _siteRepository.AddAsync(site);
await _unitOfWork.SaveChangesAsync();

// Update
site.Name = "Updated Name";
_siteRepository.Update(site);
await _unitOfWork.SaveChangesAsync();

// Soft Delete
_siteRepository.Remove(site);
await _unitOfWork.SaveChangesAsync();
```

### Unit of Work Pattern

Manages transactions and ensures consistency:

```csharp
public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
```

**Transaction Example:**
```csharp
await _unitOfWork.BeginTransactionAsync();
try
{
    // Multiple operations
    await _siteRepository.AddAsync(site);
    await _pageRepository.AddAsync(page);
    await _unitOfWork.SaveChangesAsync();
    
    await _unitOfWork.CommitTransactionAsync();
}
catch
{
    await _unitOfWork.RollbackTransactionAsync();
    throw;
}
```

## Soft Delete

All entities support soft delete through the `IsDeleted` flag:

- Deleted entities are marked with `IsDeleted = true`
- Query filters automatically exclude soft-deleted entities
- Use `IgnoreQueryFilters()` to include deleted entities
- Use `HardDelete()` for physical deletion

**Example:**
```csharp
// Soft delete (recommended)
_siteRepository.Remove(site);

// Query including deleted
var allSites = await _context.Sites
    .IgnoreQueryFilters()
    .ToListAsync();

// Hard delete (use with caution)
_siteRepository.HardDelete(site);
```

## Audit Fields

All entities inherit from `BaseEntity` with automatic audit tracking:

```csharp
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedOn { get; set; }  // Auto-set on creation
    public string CreatedBy { get; set; }
    public DateTime? ModifiedOn { get; set; }  // Auto-set on update
    public string? ModifiedBy { get; set; }
    public bool IsDeleted { get; set; }
}
```

These fields are automatically populated by the `ApplicationDbContext`:
- `CreatedOn`: Set when entity is first added
- `ModifiedOn`: Updated on every change
- `CreatedBy`/`ModifiedBy`: Must be set by application code (user context)

## Migrations

### Creating New Migrations

When you modify entity models:

```bash
cd src/OrkinosaiCMS.Infrastructure
dotnet ef migrations add YourMigrationName --startup-project ../OrkinosaiCMS.Web
```

### Applying Migrations

```bash
# Apply all pending migrations
dotnet ef database update --startup-project ../OrkinosaiCMS.Web

# Apply to specific migration
dotnet ef database update TargetMigrationName --startup-project ../OrkinosaiCMS.Web

# Revert to previous migration
dotnet ef database update PreviousMigrationName --startup-project ../OrkinosaiCMS.Web
```

### Generating SQL Scripts

```bash
# Generate script for all migrations
dotnet ef migrations script --startup-project ../OrkinosaiCMS.Web --output schema.sql

# Generate script for specific range
dotnet ef migrations script FromMigration ToMigration --startup-project ../OrkinosaiCMS.Web
```

## Performance Considerations

### Query Optimization

1. **Use AsNoTracking** for read-only queries:
```csharp
var sites = await _context.Sites.AsNoTracking().ToListAsync();
```

2. **Project to DTOs** to reduce data transfer:
```csharp
var siteDtos = await _context.Sites
    .Select(s => new SiteDto { Id = s.Id, Name = s.Name })
    .ToListAsync();
```

3. **Use Include** for eager loading:
```csharp
var siteWithPages = await _context.Sites
    .Include(s => s.Pages)
    .FirstOrDefaultAsync(s => s.Id == id);
```

4. **Avoid N+1 queries** with batch loading:
```csharp
// Bad - N+1 query
foreach (var site in sites)
{
    var pages = await _context.Pages.Where(p => p.SiteId == site.Id).ToListAsync();
}

// Good - Single query
var siteIds = sites.Select(s => s.Id);
var pages = await _context.Pages.Where(p => siteIds.Contains(p.SiteId)).ToListAsync();
```

### Indexing Strategy

Current indexes cover:
- Primary keys (automatic)
- Foreign keys (automatic)
- Unique constraints (business rules)
- Common query patterns (Site.Url, Page.Path, etc.)

Add custom indexes for:
- Frequent WHERE clause columns
- ORDER BY columns
- JOIN columns

### Connection Resilience

The application is configured with retry logic for transient failures:

```csharp
options.UseSqlServer(connectionString, sqlOptions =>
{
    sqlOptions.EnableRetryOnFailure(
        maxRetryCount: 5,
        maxRetryDelay: TimeSpan.FromSeconds(30),
        errorNumbersToAdd: null);
});
```

## Security Best Practices

1. **Parameterized Queries**: EF Core uses parameterized queries by default
2. **SQL Injection Protection**: LINQ queries are safe from SQL injection
3. **Connection String Security**: 
   - Use Azure Key Vault in production
   - Never commit connection strings to source control
   - Use environment variables or app settings
4. **Least Privilege**: Database user should have minimal required permissions
5. **Encryption**: Use encrypted connections (TrustServerCertificate=False in production)

## Backup and Recovery

### Development (LocalDB)
LocalDB data is stored in: `%USERPROFILE%\AppData\Local\Microsoft\Microsoft SQL Server Local DB\Instances\MSSQLLocalDB`

### Production (Azure SQL)
- Automatic backups enabled by default
- Point-in-time restore available
- See [AZURE_DEPLOYMENT.md](./AZURE_DEPLOYMENT.md) for backup procedures

## Monitoring

### Enable SQL Logging (Development)

In `appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

### Production Monitoring

Use Application Insights to monitor:
- Query performance
- Connection failures
- Transaction duration
- Deadlocks and timeouts

## See Also

- [SETUP.md](./SETUP.md) - Setup and configuration
- [AZURE_DEPLOYMENT.md](./AZURE_DEPLOYMENT.md) - Azure deployment
- [ARCHITECTURE.md](./ARCHITECTURE.md) - Overall architecture
- [Entity Framework Core Documentation](https://docs.microsoft.com/en-us/ef/core/)
