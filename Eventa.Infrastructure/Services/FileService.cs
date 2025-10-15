using Eventa.Application.Services;
using Microsoft.AspNetCore.Hosting;

namespace Eventa.Infrastructure.Services
{
    public class FileService : IFileService
    {
        private const string filePath = "uploads";

        private static readonly string[] validFileExtensions = { ".jpg" };

        private readonly string path = string.Empty;

        public FileService(IWebHostEnvironment environment)
        {
            path = Path.Combine(environment.WebRootPath, filePath);
        }

        public string? GetFileUrl(string fileName)
        {
            var filePath = Path.Combine(path, fileName);
            return File.Exists(filePath) ? filePath : null;
        }

        public async Task<bool> SaveFile(Stream bytes, string fileName)
        {
            using var fileStream = File.Create(Path.Combine(path, fileName));

            if (fileStream == null)
            {
                return false;
            }

            await bytes.CopyToAsync(fileStream);

            return true;
        }

        public bool Exists(string fileName)
        {
            return File.Exists(Path.Combine(path, fileName));
        }

        public async Task<bool> UpdateFile(Stream bytes, string fileName)
        {
            using var fileStream = File.OpenWrite(Path.Combine(path, fileName));

            if (fileStream == null)
            {
                return false;
            }

            await bytes.CopyToAsync(fileStream);

            return true;
        }

        public void DeleteFile(string fileName)
        {
            File.Delete(Path.Combine(path, fileName));
        }

        public bool IsValidExtension(string fileName)
        {
            string extension = Path.GetExtension(fileName);

            return validFileExtensions.Contains(extension);
        }
    }
}
