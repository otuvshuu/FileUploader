using FileUploader.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Bind AzureStorageConfig from configuration
builder.Services.Configure<AzureStorageConfig>(builder.Configuration.GetSection("AzureStorageConfig"));

// Register BlobStorage as a singleton IStorage
builder.Services.AddSingleton<IStorage>(sp =>
{
    var blobStorage = new BlobStorage(sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<AzureStorageConfig>>());
    // Do not block startup here; initialization will be performed by hosted service
    return blobStorage;
});

// Hosted service to initialize container at startup
builder.Services.AddHostedService<BlobStorageInitializer>();

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

app.Run();
