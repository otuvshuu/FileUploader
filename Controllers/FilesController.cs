using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FileUploader.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

namespace FileUploader.Controllers
{
    [Route("api/[controller]")]
    public class FilesController : Controller
    {
        private const int MaxFilenameLength = 50;
        private static readonly Regex filenameRegex = new Regex("[^a-zA-Z0-9._]");

        private readonly IStorage storage;
        private readonly AzureComputerVisionService visionService;

        public FilesController(IStorage storage, AzureComputerVisionService visionService)
        {
            this.storage = storage;
            this.visionService = visionService;
        }

        // GET /api/Files
        // Called by the page when it's first loaded, whenever new files are uploaded, and every
        // five seconds on a timer.
        [HttpGet()]
        public async Task<IActionResult> Index()
        {
            var names = await storage.GetNames();

            var baseUrl = Request.Path.Value;

            var urls = names.Select(n => $"{baseUrl}/{n}");

            return Ok(urls);
        }

        // POST /api/Files
        // Called once for each file uploaded.
        [HttpPost()]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            var name = SanitizeFilename(file.FileName);
            if (String.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException();
            }
            using (Stream stream = file.OpenReadStream())
            {
                await storage.Save(stream, name);
            }
            return Accepted();
        }

        // POST /api/Files/analyze
        // Analyze an uploaded image for tags and objects using Azure Computer Vision
        [HttpPost("analyze")]
        public async Task<IActionResult> Analyze(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var imageBytes = ms.ToArray();

            var (tags, objects) = await visionService.AnalyzeImageAsync(imageBytes);

            return Ok(new { tags, objects });
        }

        // GET /api/Files/{filename}
        // Called when clicking a link to download a specific file.
        [HttpGet("{filename}")]
        public async Task<IActionResult> Download(string filename)
        {
            var stream = await storage.Load(filename);

            // This usage of File() always triggers the browser to perform a file download.
            // We always use "application/octet-stream" as the content type because we don't record
            // any information about content type from the user when they upload a file.
            return File(stream, "application/octet-stream", filename);
        }

        private static string SanitizeFilename(string filename)
        {
            var sanitizedFilename = filenameRegex.Replace(filename, "").TrimEnd('.');
            if (sanitizedFilename.Length > MaxFilenameLength)
            {
                sanitizedFilename = sanitizedFilename.Substring(0, MaxFilenameLength);
            }
            return sanitizedFilename;
        }
    }
}
