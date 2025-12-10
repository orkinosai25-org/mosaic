using MosaicCMS.Models;
using MosaicCMS.Services.Storage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Configure CORS for web clients (can be customized based on requirements)
builder.Services.AddCors(options =>
{
    options.AddPolicy("CmsPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure Azure Blob Storage options
builder.Services.Configure<AzureBlobStorageOptions>(
    builder.Configuration.GetSection("AzureBlobStorage"));

// Register Blob Storage Service as Scoped for proper lifecycle management
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();

// Register Backup Service for tenant backup and restore operations
builder.Services.AddScoped<IBackupService, BackupService>();

// Configure OpenAPI/Swagger for API documentation
builder.Services.AddOpenApi();

// Add endpoint API explorer for OpenAPI
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    // Enable OpenAPI UI in development
    app.MapOpenApi();
    
    // Enable developer exception page for detailed error information
    app.UseDeveloperExceptionPage();
}

// Enable CORS
app.UseCors("CmsPolicy");

// Enforce HTTPS redirection for security
app.UseHttpsRedirection();

// Map controllers for API endpoints
app.MapControllers();

app.Run();
