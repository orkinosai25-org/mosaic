using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using OrkinosaiCMS.Core.Interfaces.Repositories;
using OrkinosaiCMS.Core.Interfaces.Services;
using OrkinosaiCMS.Infrastructure.Data;
using OrkinosaiCMS.Infrastructure.Repositories;
using OrkinosaiCMS.Infrastructure.Services;
using OrkinosaiCMS.Web.Components;
using OrkinosaiCMS.Web.Middleware;
using OrkinosaiCMS.Web.Services;
using Serilog;
using Serilog.Events;

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Configure Serilog from appsettings.json (skip for Testing environment)
    if (builder.Environment.EnvironmentName != "Testing")
    {
        // Configure Serilog early in the pipeline for startup logging
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        Log.Information("Starting OrkinosaiCMS application");

        builder.Host.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId());
    }

    // Add services to the container.
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    // Add Controllers for API endpoints
    builder.Services.AddControllers();

    // Add Authentication and Authorization
    // Configure default authentication scheme to allow anonymous access by default
    // The custom authentication is used for admin panel authentication via Blazor's AuthenticationStateProvider
    builder.Services.AddAuthentication("DefaultAuthScheme")
        .AddCookie("DefaultAuthScheme", options =>
        {
            options.LoginPath = "/admin/login";
            options.LogoutPath = "/admin/logout";
            // Allow anonymous access by default - authentication is handled by CustomAuthenticationStateProvider
            options.Cookie.IsEssential = true;
        });
    
    // TODO: To enable Google OAuth authentication, uncomment the following code and configure in appsettings.json or environment variables
    // Production configuration example:
    // 1. Set environment variables or appsettings.json:
    //    "Authentication": {
    //      "Google": {
    //        "ClientId": "your-google-client-id.apps.googleusercontent.com",
    //        "ClientSecret": "your-google-client-secret"
    //      }
    //    }
    // 2. Uncomment and configure AddGoogle:
    //    .AddGoogle(googleOptions =>
    //    {
    //        googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? throw new InvalidOperationException("Google ClientId not configured");
    //        googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? throw new InvalidOperationException("Google ClientSecret not configured");
    //        googleOptions.CallbackPath = "/signin-google";
    //        googleOptions.SaveTokens = true;
    //    });
    // 3. Update the UI to properly redirect to the OAuth flow instead of mock implementation
    
    builder.Services.AddAuthorizationCore();
    builder.Services.AddCascadingAuthenticationState();
    builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
    builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();

    // Configure Database
    var databaseProvider = builder.Configuration.GetValue<string>("DatabaseProvider") ?? "SqlServer";

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        if (databaseProvider.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
        {
            // Use in-memory database for testing
            options.UseInMemoryDatabase("InMemoryTestDb");
        }
        else if (databaseProvider.Equals("SQLite", StringComparison.OrdinalIgnoreCase))
        {
            var sqliteConnectionString = builder.Configuration.GetConnectionString("SqliteConnection") ?? "Data Source=orkinosai-cms.db";
            options.UseSqlite(sqliteConnectionString, sqliteOptions =>
            {
                sqliteOptions.MigrationsAssembly("OrkinosaiCMS.Infrastructure");
            });
        }
        else
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
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
    builder.Services.AddScoped<ISiteService, SiteService>();
    
    // Register Subscription Services
    builder.Services.AddScoped<ICustomerService, OrkinosaiCMS.Infrastructure.Services.Subscriptions.CustomerService>();
    builder.Services.AddScoped<ISubscriptionService, OrkinosaiCMS.Infrastructure.Services.Subscriptions.SubscriptionService>();
    builder.Services.AddScoped<IStripeService, OrkinosaiCMS.Infrastructure.Services.Subscriptions.StripeService>();

    var app = builder.Build();

    // Use Serilog for request logging (skip for Testing environment)
    if (app.Environment.EnvironmentName != "Testing")
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
            options.GetLevel = (httpContext, elapsed, ex) => ex != null 
                ? LogEventLevel.Error 
                : httpContext.Response.StatusCode > 499 
                    ? LogEventLevel.Error 
                    : LogEventLevel.Information;
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress);
            };
        });
    }

    // Add global exception handler to log all unhandled exceptions
    app.UseGlobalExceptionHandler();

    // Initialize database with seed data
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Initializing database with seed data...");
            
            await SeedData.InitializeAsync(services);
            
            logger.LogInformation("Database initialization completed successfully");
        }
        catch (Microsoft.Data.SqlClient.SqlException sqlEx)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(sqlEx, 
                "SQL connection error occurred while seeding the database. " +
                "Error Number: {ErrorNumber}, Server: {Server}, Database: {Database}",
                sqlEx.Number,
                sqlEx.Server,
                sqlEx.Message);
            
            // Log connection string info (without password)
            var config = services.GetRequiredService<IConfiguration>();
            var connString = config.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrEmpty(connString))
            {
                var sanitizedConnString = System.Text.RegularExpressions.Regex.Replace(
                    connString, 
                    @"Password=[^;]*", 
                    "Password=***");
                logger.LogError("Connection string (sanitized): {ConnectionString}", sanitizedConnString);
            }
            
            logger.LogWarning("Application will continue but database may not be properly initialized. Admin login may fail.");
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred seeding the database.");
            logger.LogWarning("Application will continue but database may not be properly initialized.");
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

    app.UseAntiforgery();

    // Middleware: Serve static files from wwwroot (includes React portal assets)
    app.UseStaticFiles();

    // Authentication and Authorization middleware
    // These must be added after UseRouting (implicit) and before endpoint mapping
    // Allows anonymous access by default - admin routes use AuthorizeView with custom authentication
    app.UseAuthentication();
    app.UseAuthorization();

    // Endpoint mappings
    app.MapStaticAssets();  // Optimized static assets for Blazor
    app.MapControllers();   // API controllers for portal integration
    app.MapRazorComponents<App>()  // Blazor CMS admin routes
        .AddInteractiveServerRenderMode();

    // SPA Fallback: Serve React portal (index.html) for non-API, non-Blazor routes
    // This ensures the portal landing page shows at root URL
    app.MapFallbackToFile("index.html", CreateNoCacheStaticFileOptions());

    if (builder.Environment.EnvironmentName != "Testing")
    {
        Log.Information("OrkinosaiCMS application started successfully");
    }
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "OrkinosaiCMS application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

// Helper method to configure no-cache headers for index.html
static StaticFileOptions CreateNoCacheStaticFileOptions()
{
    return new StaticFileOptions
    {
        OnPrepareResponse = ctx =>
        {
            // Don't cache index.html to ensure fresh deployments
            ctx.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
            ctx.Context.Response.Headers.Append("Pragma", "no-cache");
            ctx.Context.Response.Headers.Append("Expires", "0");
        }
    };
}
