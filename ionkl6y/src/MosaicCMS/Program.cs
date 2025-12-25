using MosaicCMS.Models;
using MosaicCMS.Services.Storage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Configure Azure Blob Storage options
builder.Services.Configure<AzureBlobStorageOptions>(
    builder.Configuration.GetSection("AzureBlobStorage"));

// Register Blob Storage Service as Scoped for proper lifecycle management
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Map controllers for API endpoints
app.MapControllers();

app.Run();
