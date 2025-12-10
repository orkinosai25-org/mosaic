using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using OrkinosaiCMS.Core.Interfaces.Repositories;
using OrkinosaiCMS.Core.Interfaces.Services;
using OrkinosaiCMS.Infrastructure.Data;
using OrkinosaiCMS.Infrastructure.Repositories;
using OrkinosaiCMS.Infrastructure.Services;
using OrkinosaiCMS.Web.Components;
using OrkinosaiCMS.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add Controllers for API endpoints
builder.Services.AddControllers();

// Add Authentication and Authorization
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

// Configure Database
var databaseProvider = builder.Configuration.GetValue<string>("DatabaseProvider") ?? "SqlServer";
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (databaseProvider.Equals("SQLite", StringComparison.OrdinalIgnoreCase))
    {
        var sqliteConnectionString = builder.Configuration.GetConnectionString("SqliteConnection") ?? "Data Source=orkinosai-cms.db";
        options.UseSqlite(sqliteConnectionString, sqliteOptions =>
        {
            sqliteOptions.MigrationsAssembly("OrkinosaiCMS.Infrastructure");
        });
    }
    else
    {
        options.UseSqlServer(connectionString, sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
            sqlOptions.MigrationsAssembly("OrkinosaiCMS.Infrastructure");
        });
    }
});

// Register Repository Pattern
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register Services
builder.Services.AddScoped<IModuleService, ModuleService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPageService, PageService>();
builder.Services.AddScoped<IContentService, ContentService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IThemeService, ThemeService>();

var app = builder.Build();

// Initialize database with seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await SeedData.InitializeAsync(services);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the database.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

// Add middleware to protect CMS routes (require authentication)
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value ?? "/";
    
    // Check if this is a CMS or Admin route
    if (path.StartsWith("/cms-", StringComparison.OrdinalIgnoreCase) ||
        path.StartsWith("/admin", StringComparison.OrdinalIgnoreCase))
    {
        // Check if user is authenticated
        // Fix: Correct null conditional logic - true when NOT authenticated
        if (!(context.User?.Identity?.IsAuthenticated ?? false))
        {
            // Redirect unauthenticated users to login page
            context.Response.Redirect("/admin/login?returnUrl=" + Uri.EscapeDataString(path));
            return;
        }
    }
    
    await next();
});

app.UseAntiforgery();

// Middleware: Serve static files from wwwroot (includes React portal assets)
app.UseStaticFiles();

// Endpoint mappings
app.MapStaticAssets();  // Optimized static assets for Blazor
app.MapControllers();   // API controllers for portal integration

// Map Blazor CMS components with proper routing constraints
// This maps only CMS-specific routes, preventing interception of the root
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Fallback routing: Serve React portal SPA for non-CMS routes
// MapFallback is called after all other endpoints, so Blazor routes are already handled
// We only need to serve the React portal for unmatched routes
app.MapFallbackToFile("index.html", new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // Don't cache the index.html to ensure users get the latest version
        ctx.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
        ctx.Context.Response.Headers.Append("Expires", "0");
    }
});

app.Run();
