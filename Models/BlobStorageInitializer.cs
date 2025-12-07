using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FileUploader.Models
{
    // Hosted service that ensures blob container exists at startup
    public class BlobStorageInitializer : IHostedService
    {
        private readonly IServiceProvider services;
        private readonly ILogger<BlobStorageInitializer> logger;

        public BlobStorageInitializer(IServiceProvider services, ILogger<BlobStorageInitializer> logger)
        {
            this.services = services;
            this.logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = services.CreateScope();
            var storage = scope.ServiceProvider.GetService<IStorage>();
            if (storage is BlobStorage blobStorage)
            {
                logger.LogInformation("Initializing blob storage container...");
                try
                {
                    await blobStorage.Initialize();
                    logger.LogInformation("Blob storage container initialized.");
                }
                catch (System.Exception ex)
                {
                    logger.LogError(ex, "Failed to initialize blob storage container.");
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
