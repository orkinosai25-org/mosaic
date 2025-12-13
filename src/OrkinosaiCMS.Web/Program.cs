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

// Create log directory early to ensure file logging works
var logDirectory = Path.Combine(AppContext.BaseDirectory, "App_Data", "Logs");
try
{
    if (!Directory.Exists(logDirectory))
    {
        Directory.CreateDirectory(logDirectory);
        Console.WriteLine($"Created log directory: {logDirectory}");
    }
}
catch (Exception ex)
{
    // If we can't create the log directory, at least log to console
    Console.WriteLine($"WARNING: Failed to create log directory at {logDirectory}: {ex.Message}");
    Console.WriteLine("Logging will continue to console only.");
}

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Configure Serilog from appsettings.json (skip for Testing environment)
    if (builder.Environment.EnvironmentName != "Testing")
    {
        // Configure Serilog early in the pipeline for startup logging
        // Always log to console as a fallback, even if file logging fails
        var bootstrapLogger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateBootstrapLogger();

        Log.Logger = bootstrapLogger;
        Log.Information("Starting OrkinosaiCMS application");
        Log.Information("Environment: {Environment}", builder.Environment.EnvironmentName);
        Log.Information("Content root: {ContentRoot}", builder.Environment.ContentRootPath);

        try
        {
            builder.Host.UseSerilog((context, services, configuration) =>
            {
                configuration
                    .ReadFrom.Configuration(context.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .Enrich.WithThreadId()
                    // Always ensure console logging is enabled as fallback
                    .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}");
            });
            
            Log.Information("Serilog configuration completed successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to configure Serilog from appsettings.json. Using bootstrap logger.");
        }
    }
    else
    {
        Console.WriteLine("Running in Testing environment - Serilog disabled");
    }

    // Log service registration start
    if (builder.Environment.EnvironmentName != "Testing")
    {
        Log.Information("Registering application services...");
    }

    // Add services to the container.
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    // Add Controllers for API endpoints with logging filter
    builder.Services.AddControllers(options =>
    {
        // Add global filter to log model validation errors
        options.Filters.Add<OrkinosaiCMS.Web.Filters.ModelValidationLoggingFilter>();
    });

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
    
    if (builder.Environment.EnvironmentName != "Testing")
    {
        Log.Information("Configuring database with provider: {DatabaseProvider}", databaseProvider);
    }

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        if (databaseProvider.Equals("InMemory", StringComparison.OrdinalIgnoreCase))
        {
            // Use in-memory database for testing
            options.UseInMemoryDatabase("InMemoryTestDb");
            if (builder.Environment.EnvironmentName != "Testing")
            {
                Log.Information("Using InMemory database provider");
            }
        }
        else if (databaseProvider.Equals("SQLite", StringComparison.OrdinalIgnoreCase))
        {
            var sqliteConnectionString = builder.Configuration.GetConnectionString("SqliteConnection") ?? "Data Source=orkinosai-cms.db";
            options.UseSqlite(sqliteConnectionString, sqliteOptions =>
            {
                sqliteOptions.MigrationsAssembly("OrkinosaiCMS.Infrastructure");
            });
            if (builder.Environment.EnvironmentName != "Testing")
            {
                Log.Information("Using SQLite database provider with connection: {Connection}", sqliteConnectionString);
            }
        }
        else
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                var errorMsg = "Connection string 'DefaultConnection' not found.";
                if (builder.Environment.EnvironmentName != "Testing")
                {
                    Log.Fatal(errorMsg);
                }
                throw new InvalidOperationException(errorMsg);
            }
            
            // Sanitize connection string for logging (hide password)
            // Support multiple password formats: Password=, pwd=, Pwd=
            var sanitizedConnString = System.Text.RegularExpressions.Regex.Replace(
                connectionString, 
                @"(Password|Pwd|pwd)\s*=\s*[^;]*", 
                "$1=***",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            
            if (builder.Environment.EnvironmentName != "Testing")
            {
                Log.Information("Using SQL Server database provider");
                Log.Information("Connection string (sanitized): {ConnectionString}", sanitizedConnString);
            }
            
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

    if (builder.Environment.EnvironmentName != "Testing")
    {
        Log.Information("Service registration completed");
        Log.Information("Building application...");
    }

    var app = builder.Build();
    
    if (builder.Environment.EnvironmentName != "Testing")
    {
        Log.Information("Application built successfully");
    }

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
                    : httpContext.Response.StatusCode > 399
                        ? LogEventLevel.Warning  // Log 4xx errors as warnings
                        : LogEventLevel.Information;
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress);
                diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
                diagnosticContext.Set("ContentType", httpContext.Request.ContentType);
                
                // Log additional info for error responses
                if (httpContext.Response.StatusCode >= 400)
                {
                    diagnosticContext.Set("StatusCode", httpContext.Response.StatusCode);
                    diagnosticContext.Set("QueryString", httpContext.Request.QueryString.Value);
                }
            };
        });
    }

    // Add early request logging BEFORE any other middleware
    // This catches requests that fail in antiforgery, model binding, etc.
    app.UseRequestLogging();

    // Add global exception handler to log all unhandled exceptions
    app.UseGlobalExceptionHandler();
    
    // Add status code logging middleware for HTTP errors (400, 500, etc.)
    app.UseStatusCodeLogging();

    // Add endpoint routing logging to diagnose routing issues
    // This logs which endpoint each request matches (Blazor, API, Fallback, etc.)
    app.UseEndpointRoutingLogging();

    // Initialize database with seed data
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        try
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("Starting database initialization...");
            logger.LogInformation("Database Provider: {Provider}", databaseProvider);
            
            await SeedData.InitializeAsync(services);
            
            logger.LogInformation("Database initialization completed successfully");
        }
        catch (Microsoft.Data.SqlClient.SqlException sqlEx)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(sqlEx, 
                "SQL connection error occurred while seeding the database. " +
                "Error Number: {ErrorNumber}, Server: {Server}, State: {State}, LineNumber: {LineNumber}",
                sqlEx.Number,
                sqlEx.Server,
                sqlEx.State,
                sqlEx.LineNumber);
            
            // Log connection string info (without password)
            var config = services.GetRequiredService<IConfiguration>();
            var connString = config.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrEmpty(connString))
            {
                // Support multiple password formats: Password=, pwd=, Pwd=
                var sanitizedConnString = System.Text.RegularExpressions.Regex.Replace(
                    connString, 
                    @"(Password|Pwd|pwd)\s*=\s*[^;]*", 
                    "$1=***",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                logger.LogError("Connection string (sanitized): {ConnectionString}", sanitizedConnString);
            }
            
            logger.LogError("SQL Error Message: {Message}", sqlEx.Message);
            logger.LogWarning("Application will continue but database may not be properly initialized. Admin login may fail.");
            logger.LogWarning("TROUBLESHOOTING: Check that SQL Server is running, firewall allows connections, and credentials are correct.");
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred seeding the database. Type: {ExceptionType}", ex.GetType().FullName);
            
            // Log inner exceptions
            var innerEx = ex.InnerException;
            var depth = 1;
            while (innerEx != null && depth <= 3)
            {
                logger.LogError("Inner Exception (Depth {Depth}): {Type} - {Message}", 
                    depth, innerEx.GetType().FullName, innerEx.Message);
                innerEx = innerEx.InnerException;
                depth++;
            }
            
            logger.LogWarning("Application will continue but database may not be properly initialized.");
        }
    }

    // Configure the HTTP request pipeline.
    if (app.Environment.EnvironmentName != "Testing")
    {
        Log.Information("Configuring HTTP request pipeline for environment: {Environment}", app.Environment.EnvironmentName);
    }
    
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
        
        if (app.Environment.EnvironmentName != "Testing")
        {
            Log.Information("Production middleware configured (ExceptionHandler, HSTS)");
        }
    }
    else
    {
        if (app.Environment.EnvironmentName != "Testing")
        {
            Log.Information("Development middleware configured (Developer Exception Page enabled)");
        }
    }
    
    app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
    app.UseHttpsRedirection();

    // Configure antiforgery with error logging
    try
    {
        app.UseAntiforgery();
        if (app.Environment.EnvironmentName != "Testing")
        {
            Log.Information("Antiforgery middleware configured");
        }
    }
    catch (Exception ex)
    {
        if (app.Environment.EnvironmentName != "Testing")
        {
            Log.Error(ex, "Failed to configure antiforgery middleware");
        }
        throw;
    }

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
    // IMPORTANT: Blazor routes (/admin/*) and API routes (/api/*) should NOT fall through to index.html
    // The MapFallbackToFile will only catch unmatched routes, which should work correctly
    // since Blazor and API routes are mapped above. However, we need to ensure proper ordering.
    app.MapFallbackToFile("index.html", CreateNoCacheStaticFileOptions());

    if (builder.Environment.EnvironmentName != "Testing")
    {
        Log.Information("Endpoint routing configured");
        Log.Information("  - Static assets: Enabled for Blazor");
        Log.Information("  - API Controllers: Mapped for /api/* routes");
        Log.Information("  - Blazor Components: Mapped (includes /admin/login, /admin, /admin/themes)");
        Log.Information("  - SPA Fallback: index.html for unmatched routes");
        Log.Information("OrkinosaiCMS application started successfully");
        Log.Information("Ready to accept requests");
        Log.Information("Note: Blazor endpoints are registered dynamically - routing diagnostics will appear on first request");
    }
    
    app.Run();
}
catch (Exception ex)
{
    // Ensure this is logged even if Serilog hasn't been fully initialized
    var errorMessage = $"OrkinosaiCMS application terminated unexpectedly: {ex.GetType().FullName}: {ex.Message}";
    
    try
    {
        Log.Fatal(ex, errorMessage);
    }
    catch
    {
        // If Serilog fails, write to console
        Console.Error.WriteLine($"[FATAL] {errorMessage}");
        Console.Error.WriteLine($"Exception: {ex}");
    }
    
    // Also write to stderr for Azure App Service diagnostics
    Console.Error.WriteLine($"[FATAL ERROR] {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} UTC - Application Startup Failed");
    Console.Error.WriteLine($"Exception Type: {ex.GetType().FullName}");
    Console.Error.WriteLine($"Message: {ex.Message}");
    Console.Error.WriteLine($"Stack Trace: {ex.StackTrace}");
    
    // Log inner exceptions
    var innerEx = ex.InnerException;
    var depth = 1;
    while (innerEx != null && depth <= 3)
    {
        Console.Error.WriteLine($"Inner Exception (Depth {depth}): {innerEx.GetType().FullName}");
        Console.Error.WriteLine($"Message: {innerEx.Message}");
        innerEx = innerEx.InnerException;
        depth++;
    }
    
    throw;
}
finally
{
    try
    {
        Log.CloseAndFlush();
    }
    catch (Exception ex)
    {
        // If Log.CloseAndFlush fails, log to console and continue shutdown
        Console.WriteLine($"Warning: Failed to flush logs during shutdown: {ex.Message}");
    }
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
