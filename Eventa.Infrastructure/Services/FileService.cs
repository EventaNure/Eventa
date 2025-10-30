using Eventa.Application.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;

namespace Eventa.Infrastructure.Services
{
    public class FileService : IFileService
    {
        private const string filePath = "uploads";

        private const int maxFileSize = 256;

        private static readonly string[] _validFileExtensions = { ".jpg", ".jpeg", ".png", ".webp" };

        private readonly string _path = string.Empty;

        private readonly string _baseUrl;

        public FileService(IWebHostEnvironment environment, IHttpContextAccessor accessor)
        {
            var request = accessor?.HttpContext?.Request;
            if (request != null)
            {
                _baseUrl = new Uri(new Uri($"{request.Scheme}://{request.Host}"), filePath.Replace("\\", "/")).ToString();
            }

            _path = Path.Combine(environment.WebRootPath, filePath);
        }

        public string? GetFileUrl(string fileName)
        {
            string? fileNameWithExtension = null!;

            foreach (var extension in _validFileExtensions)
            {
                if (Exists(fileName + extension))
                {
                    fileNameWithExtension = fileName + extension;
                    break;
                }
            }

            if (fileNameWithExtension == null)
            {
                return null;
            }

            return new Uri(
                new Uri(_baseUrl.EndsWith('/') ? _baseUrl : _baseUrl + "/"),
                fileNameWithExtension.Replace("\\", "/")).ToString();
        }

        public void MoveFile(string oldPath, string newPath)
        {
            File.Move(Path.Combine(_path, oldPath), Path.Combine(_path, newPath));
        }

        public string? GetFileUrlWithSpecificExtension(string fileName)
        {
            if (!Exists(fileName))
            {
                return null;
            }

            return new Uri(
                new Uri(_baseUrl.EndsWith('/') ? _baseUrl : _baseUrl + "/"),
                fileName.Replace("\\", "/")).ToString();
        }

        public void ClearFolder(string folderName)
        {
            var files = Directory.GetFiles(Path.Combine(_path, folderName));
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }

        public bool Exists(string fileName)
        {
            return File.Exists(Path.Combine(_path, fileName));
        }

        public async Task<bool> SaveFile(Stream bytes, string fileName)
        {
            var fileNameWithoutExtension = Path.Combine(Path.GetDirectoryName(fileName) ?? string.Empty, Path.GetFileNameWithoutExtension(fileName));
            foreach (var extension in _validFileExtensions)
            {
                if (Exists(fileNameWithoutExtension + extension))
                {
                    DeleteFile(fileNameWithoutExtension + extension);
                    break;
                }
            }

            using var fileStream = File.Create(Path.Combine(_path, fileName));

            if (fileStream == null)
            {
                return false;
            }
            bytes.Position = 0;
            await bytes.CopyToAsync(fileStream);

            return true;
        }

        public void DeleteFile(string fileName)
        {
            File.Delete(Path.Combine(_path, fileName));
        }

        public bool IsValidExtension(string fileName)
        {
            string extension = Path.GetExtension(fileName);

            return _validFileExtensions.Contains(extension);
        }

        public bool IsValidSize(Stream bytes)
        {
            using var image = Image.Load(bytes);

            if (image.Width > maxFileSize || image.Height > maxFileSize)
            {
                return false;
            }

            return true;
        }
    }
}
