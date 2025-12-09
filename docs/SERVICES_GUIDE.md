# OrkinosaiCMS Services Guide

This guide provides detailed information about the core services implemented in OrkinosaiCMS for managing users, pages, content, roles, and permissions.

## Overview

The CMS implements a service-oriented architecture with the following key services:

- **UserService**: User account management and authentication
- **PageService**: Page creation, publishing, and navigation
- **ContentService**: Document and media content management
- **RoleService**: Role and permission management
- **PermissionService**: Permission validation and access control

All services follow the Repository pattern and use dependency injection for loose coupling.

## User Management Service (IUserService)

### Purpose
Manages user accounts, authentication, and role assignments.

### Key Features
- Create, read, update, and delete (CRUD) operations for users
- Password hashing using BCrypt for security
- User authentication and password verification
- Role assignment and management
- Last login tracking
- Active/inactive user status

### Usage Examples

```csharp
// Inject the service
public class MyComponent
{
    private readonly IUserService _userService;

    public MyComponent(IUserService userService)
    {
        _userService = userService;
    }

    // Create a new user
    public async Task CreateUser()
    {
        var user = new User
        {
            Username = "john.doe",
            Email = "john.doe@example.com",
            DisplayName = "John Doe",
            IsActive = true
        };

        await _userService.CreateAsync(user, "SecurePassword123!");
    }

    // Verify user credentials
    public async Task<bool> Login(string username, string password)
    {
        var isValid = await _userService.VerifyPasswordAsync(username, password);
        
        if (isValid)
        {
            var user = await _userService.GetByUsernameAsync(username);
            if (user != null)
            {
                await _userService.UpdateLastLoginAsync(user.Id);
            }
        }

        return isValid;
    }

    // Assign roles to user
    public async Task AssignRoles(int userId)
    {
        var roleIds = new[] { 1, 2, 3 }; // Administrator, Editor, Contributor
        await _userService.AssignRolesAsync(userId, roleIds);
    }

    // Get user's roles
    public async Task<IEnumerable<Role>> GetUserRoles(int userId)
    {
        return await _userService.GetUserRolesAsync(userId);
    }
}
```

### API Reference

| Method | Description |
|--------|-------------|
| `GetByIdAsync(int id)` | Get user by ID |
| `GetByUsernameAsync(string username)` | Get user by username |
| `GetByEmailAsync(string email)` | Get user by email |
| `GetAllAsync()` | Get all users |
| `GetActiveUsersAsync()` | Get only active users |
| `CreateAsync(User user, string password)` | Create new user with password |
| `UpdateAsync(User user)` | Update existing user |
| `DeleteAsync(int id)` | Soft delete a user |
| `AssignRolesAsync(int userId, IEnumerable<int> roleIds)` | Assign roles to user |
| `RemoveRolesAsync(int userId, IEnumerable<int> roleIds)` | Remove roles from user |
| `GetUserRolesAsync(int userId)` | Get user's roles |
| `VerifyPasswordAsync(string username, string password)` | Verify user password |
| `ChangePasswordAsync(int userId, string currentPassword, string newPassword)` | Change user password |
| `UpdateLastLoginAsync(int userId)` | Update last login timestamp |

## Page Management Service (IPageService)

### Purpose
Manages CMS pages with publish/draft workflow and hierarchical navigation.

### Key Features
- Full CRUD operations for pages
- Publish and unpublish functionality
- Hierarchical page structure (parent-child relationships)
- Page reordering
- Move pages to different parents
- Filter by published/draft status
- Site-scoped pages

### Usage Examples

```csharp
// Create a new page
public async Task<Page> CreateHomePage(int siteId)
{
    var page = new Page
    {
        SiteId = siteId,
        Title = "Home",
        Path = "/",
        Content = "<h1>Welcome to OrkinosaiCMS</h1>",
        IsPublished = false, // Start as draft
        ShowInNavigation = true,
        Order = 0
    };

    return await _pageService.CreateAsync(page);
}

// Publish a page
public async Task PublishPage(int pageId)
{
    await _pageService.PublishAsync(pageId);
}

// Get all published pages for a site
public async Task<IEnumerable<Page>> GetPublishedPages(int siteId)
{
    return await _pageService.GetPublishedPagesAsync(siteId);
}

// Create child page
public async Task CreateChildPage(int parentId, int siteId)
{
    var childPage = new Page
    {
        SiteId = siteId,
        ParentId = parentId,
        Title = "About Us",
        Path = "/about",
        IsPublished = true,
        Order = 1
    };

    await _pageService.CreateAsync(childPage);
}

// Move page to different parent
public async Task ReorganizePages(int pageId, int newParentId)
{
    await _pageService.MoveAsync(pageId, newParentId);
}
```

### API Reference

| Method | Description |
|--------|-------------|
| `GetByIdAsync(int id)` | Get page by ID (includes modules and children) |
| `GetByPathAsync(string path, int siteId)` | Get page by URL path |
| `GetAllAsync()` | Get all pages |
| `GetBySiteAsync(int siteId)` | Get all pages for a site |
| `GetPublishedPagesAsync(int siteId)` | Get published pages for a site |
| `GetDraftPagesAsync(int siteId)` | Get draft pages for a site |
| `GetChildPagesAsync(int parentId)` | Get child pages |
| `CreateAsync(Page page)` | Create new page |
| `UpdateAsync(Page page)` | Update existing page |
| `DeleteAsync(int id)` | Soft delete a page |
| `PublishAsync(int id)` | Publish a page |
| `UnpublishAsync(int id)` | Unpublish a page (set to draft) |
| `ReorderAsync(int id, int newOrder)` | Change page order |
| `MoveAsync(int id, int? newParentId)` | Move page to different parent |

## Content Management Service (IContentService)

### Purpose
Manages content entities including documents, media files, and custom content types.

### Key Features
- CRUD operations for content
- Support for multiple content types (Document, Image, Video, Custom)
- Categorization and tagging
- Publish/draft workflow
- Author tracking
- File metadata (path, MIME type, size)
- Full-text search

### Usage Examples

```csharp
// Create a document
public async Task<Content> CreateDocument(int siteId, int authorId)
{
    var content = new Content
    {
        SiteId = siteId,
        Title = "Company Policy Document",
        ContentType = "Document",
        Body = "This document outlines company policies...",
        FilePath = "/documents/company-policy.pdf",
        MimeType = "application/pdf",
        FileSize = 1024000, // 1MB
        Category = "HR",
        Tags = "policy,hr,legal",
        AuthorId = authorId,
        IsPublished = true
    };

    return await _contentService.CreateAsync(content);
}

// Upload an image
public async Task<Content> UploadImage(int siteId, int authorId)
{
    var image = new Content
    {
        SiteId = siteId,
        Title = "Company Logo",
        ContentType = "Image",
        FilePath = "/media/images/logo.png",
        MimeType = "image/png",
        FileSize = 50000,
        Category = "Branding",
        IsPublished = true,
        AuthorId = authorId
    };

    return await _contentService.CreateAsync(image);
}

// Search content
public async Task<IEnumerable<Content>> SearchContent(int siteId, string query)
{
    return await _contentService.SearchAsync(query, siteId);
}

// Get content by tags
public async Task<IEnumerable<Content>> GetContentByTags(string[] tags)
{
    return await _contentService.GetByTagsAsync(tags);
}

// Get content by author
public async Task<IEnumerable<Content>> GetMyContent(int userId)
{
    return await _contentService.GetByAuthorAsync(userId);
}
```

### API Reference

| Method | Description |
|--------|-------------|
| `GetByIdAsync(int id)` | Get content by ID |
| `GetAllAsync()` | Get all content |
| `GetBySiteAsync(int siteId)` | Get all content for a site |
| `GetByTypeAsync(string contentType)` | Get content by type |
| `GetByCategoryAsync(string category)` | Get content by category |
| `GetByTagsAsync(IEnumerable<string> tags)` | Get content by tags |
| `GetPublishedContentAsync(int siteId)` | Get published content for a site |
| `GetDraftContentAsync(int siteId)` | Get draft content for a site |
| `GetByAuthorAsync(int authorId)` | Get content by author |
| `CreateAsync(Content content)` | Create new content |
| `UpdateAsync(Content content)` | Update existing content |
| `DeleteAsync(int id)` | Soft delete content |
| `PublishAsync(int id)` | Publish content |
| `UnpublishAsync(int id)` | Unpublish content |
| `SearchAsync(string searchTerm, int siteId)` | Search content by title or body |

## Role Management Service (IRoleService)

### Purpose
Manages roles and their associated permissions.

### Key Features
- CRUD operations for roles
- Permission assignment to roles
- System role protection
- Role-permission relationship management

### Usage Examples

```csharp
// Create a custom role
public async Task<Role> CreateCustomRole()
{
    var role = new Role
    {
        Name = "Content Manager",
        Description = "Can manage content but not users or system settings",
        IsSystem = false
    };

    return await _roleService.CreateAsync(role);
}

// Assign permissions to role
public async Task AssignPermissionsToRole(int roleId)
{
    var permissionIds = new[] { 1, 2, 3, 4 }; // View, Create, Edit, Delete Content
    await _roleService.AssignPermissionsAsync(roleId, permissionIds);
}

// Get role permissions
public async Task<IEnumerable<Permission>> GetRolePermissions(int roleId)
{
    return await _roleService.GetRolePermissionsAsync(roleId);
}
```

### API Reference

| Method | Description |
|--------|-------------|
| `GetByIdAsync(int id)` | Get role by ID |
| `GetByNameAsync(string name)` | Get role by name |
| `GetAllAsync()` | Get all roles |
| `CreateAsync(Role role)` | Create new role |
| `UpdateAsync(Role role)` | Update existing role |
| `DeleteAsync(int id)` | Soft delete role (cannot delete system roles) |
| `AssignPermissionsAsync(int roleId, IEnumerable<int> permissionIds)` | Assign permissions to role |
| `RemovePermissionsAsync(int roleId, IEnumerable<int> permissionIds)` | Remove permissions from role |
| `GetRolePermissionsAsync(int roleId)` | Get role's permissions |

## Permission Management Service (IPermissionService)

### Purpose
Manages permissions and validates user access.

### Key Features
- CRUD operations for permissions
- User permission validation
- Aggregate permissions from all user roles

### Usage Examples

```csharp
// Check if user has permission
public async Task<bool> CanEditContent(int userId)
{
    return await _permissionService.UserHasPermissionAsync(userId, "Content.Edit");
}

// Get all user permissions
public async Task<IEnumerable<Permission>> GetUserPermissions(int userId)
{
    return await _permissionService.GetUserPermissionsAsync(userId);
}

// Create a custom permission
public async Task<Permission> CreatePermission()
{
    var permission = new Permission
    {
        Name = "Module.CustomAction",
        Description = "Can perform custom action on module"
    };

    return await _permissionService.CreateAsync(permission);
}
```

### API Reference

| Method | Description |
|--------|-------------|
| `GetByIdAsync(int id)` | Get permission by ID |
| `GetByNameAsync(string name)` | Get permission by name |
| `GetAllAsync()` | Get all permissions |
| `CreateAsync(Permission permission)` | Create new permission |
| `UpdateAsync(Permission permission)` | Update existing permission |
| `DeleteAsync(int id)` | Soft delete permission |
| `UserHasPermissionAsync(int userId, string permissionName)` | Check if user has permission |
| `GetUserPermissionsAsync(int userId)` | Get all permissions for a user |

## Database Configuration

### SQL Azure (Production)

Set in `appsettings.json`:

```json
{
  "DatabaseEnabled": true,
  "DatabaseProvider": "SqlServer",
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:orkinosai.database.windows.net,1433;Initial Catalog=orkinosai-cms;Persist Security Info=False;User ID=sqladmin;Password=YourPassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
}
```

### SQLite (Development/Testing)

Set in `appsettings.Development.json`:

```json
{
  "DatabaseEnabled": true,
  "DatabaseProvider": "SQLite",
  "ConnectionStrings": {
    "SqliteConnection": "Data Source=orkinosai-cms-dev.db"
  }
}
```

## Security Considerations

### Password Security
- Passwords are hashed using BCrypt with automatic salt generation
- BCrypt.Net-Next library provides industry-standard password hashing
- Never store plain text passwords

### Soft Delete
- All entities support soft delete (IsDeleted flag)
- Deleted records remain in database but are filtered from queries
- Can be recovered or permanently deleted later

### Role-Based Access Control
- Permissions are assigned to roles, not individual users
- Users inherit permissions from all their assigned roles
- System roles cannot be deleted

### Input Validation
- Always validate input data before calling service methods
- Use CancellationToken for long-running operations
- Handle exceptions appropriately (ArgumentException, InvalidOperationException)

## Best Practices

1. **Dependency Injection**: Always inject services through constructors
2. **CancellationToken**: Pass CancellationToken for async operations
3. **Error Handling**: Wrap service calls in try-catch blocks
4. **Transactions**: Use IUnitOfWork for operations that modify multiple entities
5. **Performance**: Use appropriate methods (e.g., GetActiveUsersAsync instead of filtering GetAllAsync)
6. **Security**: Always validate user permissions before performing operations

## Next Steps

- Implement API controllers to expose services via REST
- Add validation attributes to entities
- Implement audit logging for sensitive operations
- Add unit tests for all services
- Create admin UI components using Blazor

---

*Last Updated: December 2025*
