using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace FileUploader.Models
{
    // Health check that verifies blob container is accessible by attempting to list blobs
    public class BlobStorageHealthCheck : IHealthCheck
    {
        private readonly IStorage storage;

        public BlobStorageHealthCheck(IStorage storage)
        {
            this.storage = storage;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // Attempt to fetch names to verify connectivity and container existence
                await storage.GetNames();
                return HealthCheckResult.Healthy("Blob storage reachable");
            }
            catch (System.Exception ex)
            {
                return HealthCheckResult.Unhealthy("Blob storage not reachable", ex);
            }
        }
    }
}
