using Eventa.Application.Services;
using Microsoft.AspNetCore.Hosting;

namespace Eventa.Infrastructure.Services
{
    public class FileService : IFileService
    {
        private const string filePath = "uploads";

        private string path = string.Empty;

        public FileService(IWebHostEnvironment environment) {
            path = Path.Combine(environment.WebRootPath, filePath);
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
    }
}
