using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace FileUploader.Models
{
    public class BlobStorage : IStorage
    {
        private readonly AzureStorageConfig storageConfig;

        public BlobStorage(IOptions<AzureStorageConfig> storageConfig)
        {
            this.storageConfig = storageConfig.Value;
        }

        public Task Initialize()
        {
            // Create the blob container if it doesn't already exist
            var blobServiceClient = new BlobServiceClient(storageConfig.ConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(storageConfig.FileContainerName);
            return containerClient.CreateIfNotExistsAsync();
        }

        public Task Save(Stream fileStream, string name)
        {
            var blobServiceClient = new BlobServiceClient(storageConfig.ConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(storageConfig.FileContainerName);
            var blobClient = containerClient.GetBlobClient(name);

            // Upload the blob, overwrite if it already exists
            return blobClient.UploadAsync(fileStream, overwrite: true);
        }

        public async Task<IEnumerable<string>> GetNames()
        {
            var names = new List<string>();

            var blobServiceClient = new BlobServiceClient(storageConfig.ConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(storageConfig.FileContainerName);

            await foreach (BlobItem blob in containerClient.GetBlobsAsync())
            {
                names.Add(blob.Name);
            }

            return names;
        }

        public async Task<Stream> Load(string name)
        {
            var blobServiceClient = new BlobServiceClient(storageConfig.ConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(storageConfig.FileContainerName);
            var blobClient = containerClient.GetBlobClient(name);

            // Confirm the blob exists before opening the stream
            var existsResponse = await blobClient.ExistsAsync();
            if (!existsResponse.Value)
            {
                throw new FileNotFoundException($"Blob '{name}' not found.");
            }

            return await blobClient.OpenReadAsync();
        }
    }
}