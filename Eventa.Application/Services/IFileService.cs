namespace Eventa.Application.Services
{
    public interface IFileService
    {
        void DeleteFile(string fileName);
        bool Exists(string fileName);
        string? GetFileUrl(string fileName);
        Task<bool> SaveFile(Stream bytes, string fileName);
        Task<bool> UpdateFile(Stream bytes, string fileName);
    }
}