namespace Handwerker.Domain.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream content, string fileName, string contentType);
    Task DeleteFileAsync(string relativePath);
}
