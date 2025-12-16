using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OrkinosaiCMS.Core.Entities.Identity;
using OrkinosaiCMS.Core.Interfaces.Repositories;
using OrkinosaiCMS.Core.Interfaces.Services;
using OrkinosaiCMS.Infrastructure.Data;
using OrkinosaiCMS.Infrastructure.Repositories;
using OrkinosaiCMS.Infrastructure.Services;
using OrkinosaiCMS.Web.Components;
using OrkinosaiCMS.Web.Constants;
using OrkinosaiCMS.Web.Middleware;
using OrkinosaiCMS.Web.Services;
using Serilog;
using Serilog.Events;
using System.Text;

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
                try
                {
                    configuration
                        .ReadFrom.Configuration(context.Configuration)
                        .ReadFrom.Services(services)
                        .Enrich.FromLogContext()
                        .Enrich.WithMachineName()
                        .Enrich.WithThreadId()
                        // Always ensure console logging is enabled as fallback
                        .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}");
                }
                catch (Exception fileEx)
                {
                    // If file sink configuration fails (e.g., cannot create log directory),
                    // fall back to console-only logging to prevent application startup failure
                    Console.WriteLine($"WARNING: Serilog file sink configuration failed: {fileEx.Message}");
                    Console.WriteLine("Falling back to console-only logging.");
                    
                    configuration
                        .MinimumLevel.Information()
                        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Information)
                        .Enrich.FromLogContext()
                        .Enrich.WithMachineName()
                        .Enrich.WithThreadId()
                        .WriteTo.Console(outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}");
                }
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

    // Add HttpContextAccessor for accessing HttpContext in services
    builder.Services.AddHttpContextAccessor();

    // Add Controllers for API endpoints with logging filter
    builder.Services.AddControllers(options =>
    {
        // Add global filter to log model validation errors
        options.Filters.Add<OrkinosaiCMS.Web.Filters.ModelValidationLoggingFilter>();
    });

    // Configure Antiforgery for Blazor Server
    // This is REQUIRED for Blazor Server forms and interactive components
    builder.Services.AddAntiforgery(options =>
    {
        // Configure cookie settings for Azure App Service deployments
        options.Cookie.Name = AuthenticationConstants.AntiforgeryCookieName;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Allow HTTP in dev, require HTTPS in prod
        options.Cookie.SameSite = SameSiteMode.Strict;
        // Make cookie essential so it's not blocked by consent requirements
        options.Cookie.IsEssential = true;
    });

    // Configure Data Protection for Azure App Service multi-instance deployments
    // Without this, antiforgery tokens will fail when load balanced across multiple instances
    // Each instance generates different keys, causing validation failures
    //
    // AZURE APP SERVICE NOTE:
    // - ContentRootPath in Azure maps to /home/site/wwwroot which is persistent storage
    // - This directory is shared across scale-out instances via Azure Files
    // - Keys persist across app restarts and deployments
    // - For production with multiple App Services, consider Azure Blob Storage or Key Vault:
    //   .PersistKeysToAzureBlobStorage(blobUri)
    //   .ProtectKeysWithAzureKeyVault(keyId, credential)
    var dataProtectionPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "DataProtection-Keys");
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionPath))
        .SetApplicationName("OrkinosaiCMS");
    
    // Ensure data protection directory exists
    try
    {
        if (!Directory.Exists(dataProtectionPath))
        {
            Directory.CreateDirectory(dataProtectionPath);
            if (builder.Environment.EnvironmentName != "Testing")
            {
                Log.Information("Created data protection keys directory: {Path}", dataProtectionPath);
            }
        }
    }
    catch (Exception ex)
    {
        if (builder.Environment.EnvironmentName != "Testing")
        {
            Log.Warning(ex, "Failed to create data protection keys directory at {Path}. Keys will be stored in memory (not recommended for production)", dataProtectionPath);
        }
    }

    // Add ASP.NET Core Identity (following Oqtane's approach)
    // This provides battle-tested authentication with UserManager and SignInManager
    builder.Services.AddIdentity<ApplicationUser, IdentityRole<int>>(options =>
    {
        // Password settings (following Oqtane's defaults)
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        
        // Lockout settings
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
        options.Lockout.MaxFailedAccessAttempts = 10;
        options.Lockout.AllowedForNewUsers = true;
        
        // User settings
        options.User.RequireUniqueEmail = true;
        
        // Sign in settings
        options.SignIn.RequireConfirmedEmail = false; // Can be enabled later
        options.SignIn.RequireConfirmedPhoneNumber = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>();

    // Configure the Identity application cookie
    // Note: Identity automatically registers authentication with "Identity.Application" as the scheme name
    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.LoginPath = "/admin/login";
        options.LogoutPath = "/admin/logout";
        options.AccessDeniedPath = "/admin/login";
        options.Cookie.Name = AuthenticationConstants.DefaultAuthScheme;
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SameSite = SameSiteMode.Lax; // Oqtane uses Lax for better compatibility
        options.ExpireTimeSpan = TimeSpan.FromHours(AuthenticationConstants.DefaultCookieExpirationHours);
        options.SlidingExpiration = true;
    });
    
    // Configure JWT Bearer authentication (following Oqtane's pattern for API clients)
    // Cookie auth is used for Blazor admin portal, JWT for API clients, mobile apps, and external integrations
    var jwtSecretKey = builder.Configuration["Authentication:Jwt:SecretKey"];
    if (!string.IsNullOrEmpty(jwtSecretKey) && jwtSecretKey.Length >= 32)
    {
        var key = Encoding.UTF8.GetBytes(jwtSecretKey);
        var issuer = builder.Configuration["Authentication:Jwt:Issuer"] ?? "OrkinosaiCMS";
        var audience = builder.Configuration["Authentication:Jwt:Audience"] ?? "OrkinosaiCMS.API";

        builder.Services.AddAuthentication()
            .AddJwtBearer(AuthenticationConstants.JwtBearerScheme, options =>
            {
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                // Configure events for logging and debugging
                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        if (builder.Environment.EnvironmentName != "Testing")
                        {
                            Log.Warning("JWT authentication failed: {Error}", context.Exception.Message);
                        }
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        if (builder.Environment.EnvironmentName != "Testing")
                        {
                            var username = context.Principal?.Identity?.Name;
                            Log.Information("JWT token validated for user: {Username}", username);
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        if (builder.Environment.EnvironmentName != "Testing")
        {
            Log.Information("JWT Bearer authentication configured (for API clients)");
        }
    }
    else
    {
        if (builder.Environment.EnvironmentName != "Testing")
        {
            Log.Warning("JWT authentication not configured - SecretKey missing or too short (must be 32+ chars)");
            Log.Information("Cookie authentication only (Blazor admin portal)");
        }
    }
    
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
    //    builder.Services.AddAuthentication()
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
    builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
    
    // Register Identity user seeder
    builder.Services.AddScoped<IdentityUserSeeder>();

    // Configure Database
    // Database provider selection priority:
    // 1. Environment variable: DatabaseProvider (e.g., set in Azure App Service Configuration)
    // 2. appsettings.{Environment}.json (e.g., appsettings.Production.json)
    // 3. appsettings.json
    // 4. Default: SqlServer
    //
    // Production/CI: Should use "SqlServer" with DefaultConnection to Azure SQL Database
    // Testing: Uses "InMemory" (set in CustomWebApplicationFactory)
    // Development: Can use "SqlServer" (LocalDB) or "SQLite" for local development
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

    // Add Health Checks for deployment verification
    builder.Services.AddHealthChecks()
        .AddDbContextCheck<ApplicationDbContext>("database");

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
            
            // Attempt to initialize database and apply migrations
            // This will throw an exception if migrations fail for SQL Server/SQLite
            // but will succeed with fallback for InMemory (testing)
            await SeedData.InitializeAsync(services);
            
            // Validate database state before attempting to seed Identity users
            // This provides clear error messages if AspNetUsers table is missing
            logger.LogInformation("Validating database state...");
            var context = services.GetRequiredService<ApplicationDbContext>();
            var validator = new OrkinosaiCMS.Infrastructure.Services.StartupDatabaseValidator(
                context, 
                services.GetRequiredService<ILogger<OrkinosaiCMS.Infrastructure.Services.StartupDatabaseValidator>>());
            
            var validationResult = await validator.ValidateDatabaseAsync();
            
            if (!validationResult.IsValid)
            {
                logger.LogCritical("=== DATABASE VALIDATION FAILED ===");
                logger.LogCritical("Error: {Error}", validationResult.ErrorMessage);
                logger.LogCritical("Action Required: {Action}", validationResult.ActionRequired);
                logger.LogCritical("========================================");
                logger.LogCritical("");
                logger.LogCritical("ADMIN LOGIN WILL NOT WORK until database migrations are applied.");
                logger.LogCritical("");
                
                // For Testing environment, allow app to continue (health checks, etc.)
                // For Production/Development with real database, this is a CRITICAL error
                if (IsTestingEnvironment(builder.Environment.EnvironmentName, databaseProvider))
                {
                    logger.LogCritical("The application will continue to start (testing environment), but database is not fully initialized.");
                }
                else
                {
                    logger.LogCritical("CRITICAL: Database validation failed in {Environment} environment with {Provider} provider.", 
                        builder.Environment.EnvironmentName, databaseProvider);
                    logger.LogCritical("This indicates migrations were not applied successfully.");
                    logger.LogCritical("Application startup will be ABORTED to prevent runtime errors.");
                    logger.LogCritical("");
                    throw new InvalidOperationException(
                        "Database validation failed: " + validationResult.ErrorMessage + ". " +
                        "AspNetUsers table and other Identity tables are missing. " +
                        "See DEPLOYMENT_VERIFICATION_GUIDE.md for troubleshooting. " +
                        "Action required: " + validationResult.ActionRequired);
                }
            }
            else
            {
                // Only seed Identity users if validation passed
                logger.LogInformation("Seeding Identity users...");
                var identitySeeder = services.GetRequiredService<IdentityUserSeeder>();
                await identitySeeder.SeedAsync();
                
                logger.LogInformation("Database initialization completed successfully");
            }
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
            
            // Special handling for missing AspNetUsers table (error 208)
            if (sqlEx.Number == 208 && sqlEx.Message.Contains("AspNetUsers"))
            {
                logger.LogCritical("");
                logger.LogCritical("=== CRITICAL: AspNetUsers table does not exist ===");
                logger.LogCritical("");
                logger.LogCritical("This means database migrations have NOT been applied.");
                logger.LogCritical("Admin login WILL NOT WORK until you apply migrations.");
                logger.LogCritical("");
                logger.LogCritical("Root Cause: SQL Error 208 - Invalid object name 'AspNetUsers'");
                logger.LogCritical("This exact error matches the issue in the bug report:");
                logger.LogCritical("'SQL error during login for username: admin - Error: 208, Message: Invalid object name AspNetUsers'");
                logger.LogCritical("");
                logger.LogCritical("REQUIRED ACTION:");
                logger.LogCritical("  1. Run: dotnet ef database update --startup-project src/OrkinosaiCMS.Web");
                logger.LogCritical("     OR");
                logger.LogCritical("  2. Run: bash scripts/apply-migrations.sh update");
                logger.LogCritical("");
                logger.LogCritical("See DEPLOYMENT_VERIFICATION_GUIDE.md for detailed instructions.");
                logger.LogCritical("====================================================");
                logger.LogCritical("");
                
                // Rethrow to prevent app from starting in broken state
                throw;
            }
            else
            {
                logger.LogError("Unhandled SQL error during database initialization.");
                logger.LogError("TROUBLESHOOTING: Check that SQL Server is running, firewall allows connections, and credentials are correct.");
                // Rethrow to prevent app from starting in broken state
                throw;
            }
        }
        catch (InvalidOperationException invEx) when (invEx.Message.Contains("migration failed") || invEx.Message.Contains("validation failed"))
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogCritical(invEx, "Database migration or validation failed - application cannot start");
            logger.LogCritical("This prevents the application from starting in a broken state where admin login would fail.");
            logger.LogCritical("See error details above for specific remediation steps.");
            
            // Rethrow to prevent app from starting
            throw;
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
            
            // For Testing environment, allow app to continue
            // For Production/Development with real database, abort startup
            if (IsTestingEnvironment(builder.Environment.EnvironmentName, databaseProvider))
            {
                logger.LogWarning("Application will continue (testing environment) but database may not be properly initialized.");
            }
            else
            {
                logger.LogCritical("Database initialization failed in {Environment} environment - aborting startup", 
                    builder.Environment.EnvironmentName);
                throw;
            }
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

    // Middleware: Serve static files from wwwroot (includes React portal assets)
    app.UseStaticFiles();

    // Authentication and Authorization middleware
    // These must be added after UseRouting (implicit) and before endpoint mapping
    // Allows anonymous access by default - admin routes use AuthorizeView with custom authentication
    app.UseAuthentication();
    app.UseAuthorization();

    // Configure antiforgery AFTER authentication/authorization and BEFORE endpoint mapping
    // This is critical for Blazor Server forms to work correctly
    // Antiforgery middleware validates tokens on POST requests
    try
    {
        app.UseAntiforgery();
        if (app.Environment.EnvironmentName != "Testing")
        {
            Log.Information("Antiforgery middleware configured (after authentication)");
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

    // Endpoint mappings
    app.MapHealthChecks("/api/health");  // Health check endpoint for deployment verification
    app.MapControllers();  // API controllers for portal integration
    
    // Map Blazor components to specific routes only (admin and cms paths)
    app.MapRazorComponents<App>()  // Blazor CMS admin routes
        .AddInteractiveServerRenderMode();

    // SPA Fallback: Serve React portal (index.html) for root and other non-Blazor routes
    // This ensures the portal landing page shows at root URL
    // IMPORTANT: This should catch any route that's not handled by Blazor or API controllers
    // The fallback will serve index.html for unmatched routes, allowing the React router to handle them
    // NOTE: MapStaticAssets() was removed to avoid conflicting fallback routes
    // Static files are still served via UseStaticFiles() middleware configured earlier
    // NOTE: In test environment, index.html may not exist, so root (/) will return 404
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

// Helper method to determine if we're in a test environment (allows InMemory database fallback)
static bool IsTestingEnvironment(string environmentName, string? databaseProvider)
{
    return environmentName == "Testing" || 
           databaseProvider?.Equals("InMemory", StringComparison.OrdinalIgnoreCase) == true;
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
