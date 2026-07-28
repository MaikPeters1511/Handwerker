using Handwerker.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Handwerker.Infrastructure.Services;

public class LocalFileStorageService(
    ILogger<LocalFileStorageService> logger,
    string webRootPath) : IFileStorageService
{
    private readonly string _baseFolder = "uploads/companies";

    public async Task<string> SaveFileAsync(Stream content, string fileName, string contentType)
    {
        var ext = Path.GetExtension(fileName);
        var guid = Guid.NewGuid().ToString();
        var safeFileName = guid + ext;

        var folder = Path.Combine(webRootPath, _baseFolder);
        Directory.CreateDirectory(folder);

        var fullPath = Path.Combine(folder, safeFileName);
        
        logger.LogInformation("Saving file to: {FullPath}", fullPath);
        
        using var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        await content.CopyToAsync(fs);

        var relative = $"/{_baseFolder}/{safeFileName}".Replace("\\", "/");
        
        logger.LogInformation("File saved successfully. Relative path: {RelativePath}", relative);
        
        return relative;
    }

    public Task DeleteFileAsync(string relativePath)
    {
        try
        {
            var p = relativePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(webRootPath, p);
            
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                logger.LogInformation("File deleted: {FullPath}", fullPath);
            }
            else
            {
                logger.LogWarning("File not found for deletion: {FullPath}", fullPath);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error deleting file {path}", relativePath);
        }

        return Task.CompletedTask;
    }
}
