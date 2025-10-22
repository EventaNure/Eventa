namespace Eventa.Application.Services
{
    public interface IFileService
    {
        void ClearFolder(string folderName);
        void DeleteFile(string fileName);
        bool Exists(string fileName);
        string? GetFileUrl(string fileName);
        string? GetFileUrlWithSpecificExtension(string fileName);
        bool IsValidExtension(string fileName);
        bool IsValidSize(Stream bytes);
        Task<bool> SaveFile(Stream bytes, string fileName);
    }
}