using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RidersApp.Services
{
    public class FileService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        public FileService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string> UploadEmployeeImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("No file provided or file is empty.");
            }

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(extension))
            {
                throw new ArgumentException("Only JPG, PNG, and GIF files are allowed.");
            }

            // Validate file size (10MB limit)
            const int maxFileSizeBytes = 10 * 1024 * 1024; // 10MB
            if (file.Length > maxFileSizeBytes)
            {
                throw new ArgumentException("File size cannot exceed 10MB.");
            }

            try
            {
                // Create unique filename
                var fileName = $"{Guid.NewGuid():N}{extension}";
                
                // Ensure upload directory exists
                var uploadsDirectory = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "employees");
                
                if (!Directory.Exists(uploadsDirectory))
                {
                    Directory.CreateDirectory(uploadsDirectory);
                }

                // Full file path
                var filePath = Path.Combine(uploadsDirectory, fileName);

                // Save file to disk
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Return the URL path (relative to web root)
                var urlPath = $"/uploads/employees/{fileName}";
                
                return urlPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FileService: Error uploading file: {ex.Message}");
                throw new Exception($"Failed to upload file: {ex.Message}", ex);
            }
        }

        public bool DeleteEmployeeImage(string pictureUrl)
        {
            if (string.IsNullOrEmpty(pictureUrl) || 
                pictureUrl.Contains("default-profile.png") ||
                !pictureUrl.StartsWith("/uploads/employees/"))
            {
                return false; // Don't delete default images or invalid paths
            }

            try
            {
                var filePath = Path.Combine(_webHostEnvironment.WebRootPath, pictureUrl.TrimStart('/'));
                
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Console.WriteLine($"FileService: Deleted file: {filePath}");
                    return true;
                }
                else
                {
                    Console.WriteLine($"FileService: File not found for deletion: {filePath}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FileService: Error deleting file {pictureUrl}: {ex.Message}");
                return false;
            }
        }

        public string GetPhysicalPath(string pictureUrl)
        {
            if (string.IsNullOrEmpty(pictureUrl))
                return null;

            return Path.Combine(_webHostEnvironment.WebRootPath, pictureUrl.TrimStart('/'));
        }

        public bool FileExists(string pictureUrl)
        {
            if (string.IsNullOrEmpty(pictureUrl))
                return false;

            var physicalPath = GetPhysicalPath(pictureUrl);
            return File.Exists(physicalPath);
        }
    }
}