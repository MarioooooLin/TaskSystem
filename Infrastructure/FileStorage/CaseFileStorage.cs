using Application.FileStorage;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.FileStorage;

/// <summary>案件附件本地磁碟儲存實作。</summary>
public sealed class CaseFileStorage : ICaseFileStorage
{
    private readonly string _rootPath;

    public CaseFileStorage(IConfiguration configuration)
    {
        var configuredRoot = configuration["FileStorage:UploadRootPath"];
        _rootPath = string.IsNullOrWhiteSpace(configuredRoot)
            ? Path.Combine(AppContext.BaseDirectory, "uploads")
            : configuredRoot;

        if (!Path.IsPathRooted(_rootPath))
        {
            _rootPath = Path.Combine(AppContext.BaseDirectory, _rootPath);
        }
    }

    public async Task<CaseStorageFileInfo> SaveAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        long fileSize,
        long uploadedByUserId,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(fileStream);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        var relativeDir = Path.Combine(
            "cases",
            DateTime.UtcNow.ToString("yyyyMM"),
            uploadedByUserId.ToString());

        var safeName = string.Concat(
            Path.GetFileNameWithoutExtension(fileName)
                .Where(c => !Path.GetInvalidFileNameChars().Contains(c)));

        if (string.IsNullOrWhiteSpace(safeName))
        {
            safeName = "file";
        }

        var ext = Path.GetExtension(fileName);
        var storageFileName = $"{safeName}_{Guid.NewGuid():N}{ext}";
        var relativePath = Path.Combine(relativeDir, storageFileName).Replace('\\', '/');
        var absolutePath = GetAbsolutePath(relativePath);

        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);

        await using var targetStream = File.Create(absolutePath);
        await fileStream.CopyToAsync(targetStream, ct);

        return new CaseStorageFileInfo
        {
            FileName = fileName,
            FileSize = (int)fileSize,
            MimeType = contentType,
            RelativePath = relativePath
        };
    }

    public Task<CaseStorageFileContent> OpenAsync(
        string relativePath,
        CancellationToken ct = default)
    {
        var absolutePath = GetAbsolutePath(relativePath);

        if (!File.Exists(absolutePath))
        {
            return Task.FromResult(new CaseStorageFileContent());
        }

        var info = new FileInfo(absolutePath);
        var stream = File.OpenRead(absolutePath);

        return Task.FromResult(new CaseStorageFileContent
        {
            Stream = stream,
            FileName = info.Name,
            MimeType = "application/octet-stream",
            Length = info.Length
        });
    }

    public Task DeleteAsync(
        string relativePath,
        CancellationToken ct = default)
    {
        var absolutePath = GetAbsolutePath(relativePath);

        if (File.Exists(absolutePath))
        {
            File.Delete(absolutePath);
        }

        return Task.CompletedTask;
    }

    private string GetAbsolutePath(string relativePath)
    {
        return Path.Combine(_rootPath, relativePath.TrimStart('/', '\\'));
    }
}
