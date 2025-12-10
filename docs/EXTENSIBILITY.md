# OrkinosaiCMS Extensibility Guide

## Overview

OrkinosaiCMS is designed from the ground up to be extensible. This guide covers all the ways you can extend and customize the CMS to meet your specific needs.

## Extension Points

1. **Custom Modules** - Create reusable components
2. **Custom Themes** - Design visual appearances
3. **Custom Master Pages** - Create layout structures
4. **Custom Services** - Add business logic
5. **Custom Entities** - Extend the data model
6. **Event Handlers** - Hook into CMS events (planned)
7. **Custom Middleware** - Add HTTP pipeline components

## Creating Custom Modules

### Module Structure

A module is a Blazor Razor Class Library that implements the `IModule` interface or inherits from `ModuleBase`.

### Step-by-Step Guide

#### 1. Create the Module Project

```bash
dotnet new razorclasslib -n OrkinosaiCMS.Modules.Blog -o src/Modules/OrkinosaiCMS.Modules.Blog --framework net10.0
cd src/Modules/OrkinosaiCMS.Modules.Blog
dotnet add reference ../../OrkinosaiCMS.Modules.Abstractions
dotnet add reference ../../OrkinosaiCMS.Core
```

#### 2. Create the Module Component

`BlogModule.razor`:

```razor
@using OrkinosaiCMS.Modules.Abstractions
@using Microsoft.Extensions.Logging
@inject ILogger<BlogModule> Logger
@inherits ModuleBase

<div class="blog-module">
    <h2>@ModuleTitle</h2>
    
    @if (Posts.Any())
    {
        <div class="blog-posts">
            @foreach (var post in Posts)
            {
                <article class="blog-post">
                    <h3>@post.Title</h3>
                    <p class="meta">@post.PublishedDate.ToShortDateString()</p>
                    <div class="content">@((MarkupString)post.Content)</div>
                </article>
            }
        </div>
    }
    else
    {
        <p>No blog posts available.</p>
    }
</div>

@code {
    [Parameter]
    public string ModuleTitle { get; set; } = "Blog";
    
    private List<BlogPost> Posts { get; set; } = new();
    
    public override string ModuleName => "Blog";
    public override string Title => "Blog Module";
    public override string Description => "Displays blog posts";
    public override string Category => "Content";
    public override string Version => "1.0.0";
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadPostsAsync();
    }
    
    private async Task LoadPostsAsync()
    {
        // Load from settings or database
        if (Settings != null && Settings.TryGetValue("PostCount", out var countObj))
        {
            var count = Convert.ToInt32(countObj);
            // Load posts from your service
        }
        
        Logger.LogInformation("Loaded {Count} blog posts", Posts.Count);
    }
    
    public class BlogPost
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime PublishedDate { get; set; }
    }
}
```

#### 3. Add Module Attribute

Add the `[Module]` attribute to enable auto-discovery:

```csharp
[Module("Blog", "Blog Module", 
    Description = "Displays blog posts with rich formatting",
    Category = "Content",
    Version = "1.0.0")]
public class BlogModule : ModuleBase
{
    // ... implementation
}
```

#### 4. Create Module Settings Component

`BlogSettings.razor`:

```razor
@using OrkinosaiCMS.Modules.Abstractions

<div class="module-settings">
    <div class="form-group">
        <label>Number of Posts to Display</label>
        <input type="number" class="form-control" @bind="PostCount" />
    </div>
    
    <div class="form-group">
        <label>Show Excerpts Only</label>
        <input type="checkbox" @bind="ShowExcerpts" />
    </div>
    
    <button class="btn btn-primary" @onclick="SaveSettings">Save Settings</button>
</div>

@code {
    [Parameter]
    public IDictionary<string, object>? Settings { get; set; }
    
    [Parameter]
    public EventCallback<IDictionary<string, object>> OnSettingsChanged { get; set; }
    
    private int PostCount { get; set; } = 10;
    private bool ShowExcerpts { get; set; }
    
    protected override void OnInitialized()
    {
        LoadSettings();
    }
    
    private void LoadSettings()
    {
        if (Settings != null)
        {
            if (Settings.TryGetValue("PostCount", out var count))
                PostCount = Convert.ToInt32(count);
            if (Settings.TryGetValue("ShowExcerpts", out var excerpts))
                ShowExcerpts = Convert.ToBoolean(excerpts);
        }
    }
    
    private async Task SaveSettings()
    {
        var newSettings = new Dictionary<string, object>
        {
            { "PostCount", PostCount },
            { "ShowExcerpts", ShowExcerpts }
        };
        
        await OnSettingsChanged.InvokeAsync(newSettings);
    }
}
```

#### 5. Add Module Styles

`BlogModule.razor.css`:

```css
.blog-module {
    padding: 1rem;
}

.blog-posts {
    display: grid;
    gap: 2rem;
}

.blog-post {
    border: 1px solid #e0e0e0;
    border-radius: 8px;
    padding: 1.5rem;
    background: white;
}

.blog-post h3 {
    margin-top: 0;
    color: #333;
}

.blog-post .meta {
    color: #666;
    font-size: 0.875rem;
    margin-bottom: 1rem;
}

.blog-post .content {
    line-height: 1.6;
}
```

### Advanced Module Features

#### Database Integration

Create a service for data access:

```csharp
public interface IBlogService
{
    Task<List<BlogPost>> GetPostsAsync(int moduleId, int count = 10);
    Task<BlogPost?> GetPostAsync(int id);
    Task<BlogPost> CreatePostAsync(BlogPost post);
    Task UpdatePostAsync(BlogPost post);
    Task DeletePostAsync(int id);
}

public class BlogService : IBlogService
{
    private readonly ApplicationDbContext _context;
    
    public BlogService(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<BlogPost>> GetPostsAsync(int moduleId, int count = 10)
    {
        return await _context.BlogPosts
            .Where(p => p.ModuleId == moduleId && p.IsPublished)
            .OrderByDescending(p => p.PublishedDate)
            .Take(count)
            .ToListAsync();
    }
    
    // ... other methods
}
```

Register the service in `Program.cs`:

```csharp
builder.Services.AddScoped<IBlogService, BlogService>();
```

#### Module Permissions

```csharp
[Module("Blog", "Blog Module")]
[RequiredPermission("Blog.View")]
public class BlogModule : ModuleBase
{
    [Parameter]
    public bool CanEdit { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        
        // Check if user has edit permission
        // CanEdit = await PermissionService.HasPermissionAsync("Blog.Edit");
    }
}
```

## Creating Custom Themes

### Theme Structure

A theme package includes:
- CSS files
- Images and icons
- JavaScript files (optional)
- Font files (optional)

### Step 1: Create Theme Directory

```
wwwroot/
  themes/
    MyTheme/
      css/
        theme.css
        variables.css
      images/
        logo.png
      js/
        theme.js
```

### Step 2: Define Theme Styles

`theme.css`:

```css
/* MyTheme Styles */
:root {
    --primary-color: #007bff;
    --secondary-color: #6c757d;
    --success-color: #28a745;
    --danger-color: #dc3545;
    --font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
}

body {
    font-family: var(--font-family);
    color: #333;
}

.navbar {
    background-color: var(--primary-color);
}

.btn-primary {
    background-color: var(--primary-color);
    border-color: var(--primary-color);
}

/* ... more styles */
```

### Step 3: Register Theme

Create a theme entity:

```csharp
var theme = new Theme
{
    Name = "MyTheme",
    Description = "A modern, clean theme",
    Version = "1.0.0",
    Author = "Your Name",
    AssetsPath = "/themes/MyTheme",
    IsEnabled = true,
    CreatedBy = "System",
    CreatedOn = DateTime.UtcNow
};
```

## Creating Custom Master Pages

### Master Page Component

Create a Blazor layout component:

`TwoColumnMasterPage.razor`:

```razor
@inherits LayoutComponentBase

<div class="master-page two-column">
    <header class="site-header">
        <div class="container">
            <ModuleZone Zone="Header" />
        </div>
    </header>
    
    <div class="site-content container">
        <div class="row">
            <aside class="sidebar col-md-3">
                <ModuleZone Zone="Sidebar" />
            </aside>
            
            <main class="main-content col-md-9">
                <ModuleZone Zone="Main" />
                @Body
            </main>
        </div>
    </div>
    
    <footer class="site-footer">
        <div class="container">
            <ModuleZone Zone="Footer" />
        </div>
    </footer>
</div>

@code {
    // Master page logic
}
```

### Register Master Page

```csharp
var masterPage = new MasterPage
{
    SiteId = siteId,
    Name = "TwoColumn",
    Description = "Two column layout with sidebar",
    ComponentPath = "Layouts.TwoColumnMasterPage",
    ContentZones = "[\"Header\",\"Sidebar\",\"Main\",\"Footer\"]",
    IsDefault = false,
    CreatedBy = "System",
    CreatedOn = DateTime.UtcNow
};
```

## Custom Services

### Creating a Custom Service

#### 1. Define Interface

`ICustomService.cs`:

```csharp
public interface ICustomService
{
    Task<CustomData> GetDataAsync(int id);
    Task<CustomData> SaveDataAsync(CustomData data);
}
```

#### 2. Implement Service

`CustomService.cs`:

```csharp
public class CustomService : ICustomService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CustomService> _logger;
    
    public CustomService(ApplicationDbContext context, ILogger<CustomService> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task<CustomData> GetDataAsync(int id)
    {
        _logger.LogInformation("Getting data for ID: {Id}", id);
        return await _context.CustomData.FindAsync(id);
    }
    
    public async Task<CustomData> SaveDataAsync(CustomData data)
    {
        _context.CustomData.Update(data);
        await _context.SaveChangesAsync();
        return data;
    }
}
```

#### 3. Register Service

In `Program.cs`:

```csharp
builder.Services.AddScoped<ICustomService, CustomService>();
```

## Custom Entities

### Extending the Data Model

#### 1. Create Entity Class

```csharp
public class BlogPost : BaseEntity
{
    public int ModuleId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Excerpt { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; }
    public bool IsPublished { get; set; }
    public string AuthorId { get; set; } = string.Empty;
    
    // Navigation properties
    public PageModule? Module { get; set; }
}
```

#### 2. Add to DbContext

```csharp
public DbSet<BlogPost> BlogPosts => Set<BlogPost>();
```

#### 3. Configure Entity

```csharp
public class BlogPostConfiguration : IEntityTypeConfiguration<BlogPost>
{
    public void Configure(EntityTypeBuilder<BlogPost> builder)
    {
        builder.HasKey(b => b.Id);
        
        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(b => b.Content)
            .IsRequired();
        
        builder.HasIndex(b => b.PublishedDate);
        
        builder.HasOne(b => b.Module)
            .WithMany()
            .HasForeignKey(b => b.ModuleId);
    }
}
```

#### 4. Create Migration

```bash
dotnet ef migrations add AddBlogPosts --startup-project ../OrkinosaiCMS.Web
dotnet ef database update --startup-project ../OrkinosaiCMS.Web
```

## Custom Middleware

### Creating Middleware

```csharp
public class CustomMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CustomMiddleware> _logger;
    
    public CustomMiddleware(RequestDelegate next, ILogger<CustomMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        _logger.LogInformation("Custom middleware executing");
        
        // Before next middleware
        await _next(context);
        
        // After next middleware
    }
}

// Extension method
public static class CustomMiddlewareExtensions
{
    public static IApplicationBuilder UseCustomMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CustomMiddleware>();
    }
}
```

### Register Middleware

In `Program.cs`:

```csharp
app.UseCustomMiddleware();
```

## Best Practices

### Module Development

1. **Keep modules focused**: One responsibility per module
2. **Use dependency injection**: Inject services, don't create them
3. **Handle errors gracefully**: Use try-catch and logging
4. **Make settings configurable**: Use the Settings dictionary
5. **Test thoroughly**: Unit and integration tests
6. **Document well**: XML comments and README

### Performance

1. **Cache appropriately**: Use IMemoryCache for frequently accessed data
2. **Async all the way**: Use async/await consistently
3. **Lazy load**: Only load what's needed
4. **Optimize queries**: Use appropriate EF Core methods
5. **Minimize re-renders**: Use `@key` directive in Blazor

### Security

1. **Validate input**: Always validate user input
2. **Check permissions**: Verify user has required permissions
3. **Sanitize HTML**: Use Html.Sanitizer for user content
4. **Prevent injection**: Use parameterized queries
5. **Log security events**: Track authentication and authorization

## Example: Complete Module Package

### Project Structure

```
OrkinosaiCMS.Modules.Blog/
  ├── Components/
  │   ├── BlogModule.razor
  │   ├── BlogModule.razor.css
  │   ├── BlogSettings.razor
  │   └── BlogPost.razor
  ├── Services/
  │   ├── IBlogService.cs
  │   └── BlogService.cs
  ├── Models/
  │   ├── BlogPost.cs
  │   └── BlogCategory.cs
  ├── Data/
  │   └── BlogPostConfiguration.cs
  └── OrkinosaiCMS.Modules.Blog.csproj
```

### Module Registration

```csharp
public static class BlogModuleExtensions
{
    public static IServiceCollection AddBlogModule(this IServiceCollection services)
    {
        services.AddScoped<IBlogService, BlogService>();
        return services;
    }
}
```

In `Program.cs`:

```csharp
builder.Services.AddBlogModule();
```

## Resources

- [Blazor Documentation](https://docs.microsoft.com/en-us/aspnet/core/blazor/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [Dependency Injection](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection)
- [Architecture Guide](ARCHITECTURE.md)
- [Setup Guide](SETUP.md)

## Community Modules

Check the [OrkinosaiCMS Module Gallery](https://github.com/orkinosai25-org/orkinosaiCMS-modules) for community-contributed modules.

## Support

- GitHub Issues: [Report bugs or request features](https://github.com/orkinosai25-org/orkinosaiCMS/issues)
- Discussions: [Ask questions](https://github.com/orkinosai25-org/orkinosaiCMS/discussions)
- Documentation: [Read the docs](https://docs.orkinosaicms.org)
