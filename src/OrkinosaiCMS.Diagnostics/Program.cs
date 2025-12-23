using Serilog;
using OrkinosaiCMS.Diagnostics.Services;

// Configure Serilog for diagnostic app logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("App_Data/Logs/diagnostics-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting OrkinosaiCMS Diagnostics Service");
    
    var builder = WebApplication.CreateBuilder(args);
    
    // Use Serilog for logging
    builder.Host.UseSerilog();
    
    // Add services
    builder.Services.AddControllers();
    builder.Services.AddRazorPages();
    
    // Register diagnostic services
    builder.Services.AddScoped<IDiagnosticDataService, DiagnosticDataService>();
    
    // Configure CORS for standalone access
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });
    
    var app = builder.Build();
    
    // Configure middleware
    app.UseCors();
    app.UseStaticFiles();
    app.UseRouting();
    
    // Map endpoints
    app.MapControllers();
    app.MapRazorPages();
    app.MapGet("/", () => Results.Redirect("/diagnostics"));
    
    Log.Information("OrkinosaiCMS Diagnostics Service started successfully");
    Log.Information("Accessible at: http://localhost:5001");
    Log.Information("Dashboard URL: http://localhost:5001/diagnostics");
    
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Diagnostics Service failed to start");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
