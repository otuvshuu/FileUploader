# FileUploader (Azure Blob Storage sample)

Simple Razor Pages app that uploads and downloads files to Azure Blob Storage.

## Overview
- Framework: .NET 10 (`net10.0`)
- UI: Razor Pages
- Storage: Azure Blob Storage via `Azure.Storage.Blobs`

## Prerequisites
- .NET 10 SDK installed
- Azure CLI logged in (`az login`)
- An Azure Storage account

## Configuration
Configure the storage settings in `appsettings.json` or set environment/app settings in Azure.

appsettings.json example:

```json
"AzureStorageConfig": {
  "ConnectionString": "<your-storage-connection-string>",
  "FileContainerName": "files"
}
```

Environment / App Service setting names:
- `AzureStorageConfig__ConnectionString`
- `AzureStorageConfig__FileContainerName` (value: `files`)

## Run locally

From the project directory (where `FileUploader.csproj` is located):

```bash
dotenv restore
dotnet build
dotnet run
```

Open a browser at the URL printed by the app (typically `https://localhost:5001`).

## Deploy to Azure App Service (recommended)

1. Create an App Service plan (replace resource group and location as needed):

```bash
az appservice plan create \
  --name blob-exercise-plan \
  --resource-group [sandbox resource group name] \
  --sku FREE --location centralus
```

2. Create a Web App (choose a globally unique name):

```bash
az webapp create \
  --name <your-unique-app-name> \
  --plan blob-exercise-plan \
  --resource-group [sandbox resource group name]
```

3. Get the storage account connection string (replace storage account name):

```bash
CONNECTIONSTRING=$(az storage account show-connection-string \
  --name <your-unique-storage-account-name> \
  --output tsv)
```

4. Configure App Service application settings (set container name to `files`):

```bash
az webapp config appsettings set \
  --name <your-unique-app-name> --resource-group [sandbox resource group name] \
  --settings AzureStorageConfig:ConnectionString="$CONNECTIONSTRING" AzureStorageConfig:FileContainerName=files
```

5. Deploy the app (publish and deploy ZIP) — from project folder:

```bash
dotnet publish -c Release -o ./publish
cd publish
zip -r ../app.zip .
cd ..
az webapp deploy --resource-group [sandbox resource group name] --name <your-unique-app-name> --src-path app.zip
```

Alternatively use `az webapp up` or GitHub Actions for continuous deployment.

## Notes
- The app expects the container name `files`. The hosted initializer will create the container at startup if it does not exist.
- Do not store secrets in source control. Use App Service settings or Azure Key Vault for production secrets.
- For production, register and reuse a singleton `BlobServiceClient` instead of creating clients per call to improve performance.

## Project files of interest
- `Program.cs` — minimal host configuration and service registration
- `Models/BlobStorage.cs` — `IStorage` implementation (upload/download/list)
- `Models/BlobStorageInitializer.cs` — hosted service that creates the container at startup
- `appsettings.json` — local app configuration
- `Controllers/FilesController.cs` / Razor Pages — UI endpoints

If you want, I can add a GitHub Actions workflow to build and deploy this app automatically when you push to the repository.

cd "C:\Users\Tuvshinbat Ochirbat\source\repos\AzureBlobStorageApp\mslearn-store-data-in-azure\store-app-data-with-azure-blob-storage\src\start"
git init
git checkout -b main
git add -A
git commit -m "Initial commit"
# create repo on GitHub and push (replace <your-repo-name> and choose --public|--private)
gh repo create <your-repo-name> --public --source=. --remote=origin --push
dotnet new gitignore

