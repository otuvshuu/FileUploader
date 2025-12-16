using FileUploader.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Bind AzureStorageConfig from configuration
builder.Services.Configure<AzureStorageConfig>(builder.Configuration.GetSection("AzureStorageConfig"));

// Register AzureComputerVisionService with config from appsettings
var visionConfig = builder.Configuration.GetSection("AzureComputerVision");
// Prefer environment variables for Computer Vision config
var visionEndpoint = Environment.GetEnvironmentVariable("AzureComputerVision__Endpoint") ?? visionConfig["Endpoint"];
var visionApiKey = Environment.GetEnvironmentVariable("AzureComputerVision__ApiKey") ?? visionConfig["ApiKey"];
builder.Services.AddSingleton(new AzureComputerVisionService(
    visionEndpoint,
    visionApiKey
));

// Register BlobStorage as a singleton IStorage
builder.Services.AddSingleton<IStorage>(sp =>
{
    var blobStorage = new BlobStorage(sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AzureStorageConfig>>());
    // Do not block startup here; initialization will be performed by hosted service
    return blobStorage;
});

// Hosted service to initialize container at startup
builder.Services.AddHostedService<BlobStorageInitializer>();

// Add health checks and register a check for blob storage readiness
builder.Services.AddHealthChecks()
    .AddCheck<BlobStorageHealthCheck>("blobstorage");

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.MapDefaultControllerRoute();
app.MapControllers();
app.MapRazorPages();

// Health check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

app.Run();
