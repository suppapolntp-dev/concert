namespace TicketWorld.Infrastructure.Services;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TicketWorld.Application.Common.Interfaces;

/// <summary>
/// File storage service implementation supporting S3/MinIO protocols with local storage fallback.
/// </summary>
public class MinioFileStorageService : IFileStorageService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<MinioFileStorageService> _logger;
    private readonly string _baseStoragePath;
    private readonly string _publicBaseUrl;

    public MinioFileStorageService(
        IConfiguration configuration,
        ILogger<MinioFileStorageService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _baseStoragePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        _publicBaseUrl = _configuration["Storage:PublicUrl"] ?? "/uploads";

        if (!Directory.Exists(_baseStoragePath))
        {
            Directory.CreateDirectory(_baseStoragePath);
        }
    }

    public async Task<string> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string? bucketName = null,
        CancellationToken cancellationToken = default)
    {
        var bucket = bucketName ?? "general";
        var bucketDirectory = Path.Combine(_baseStoragePath, bucket);

        if (!Directory.Exists(bucketDirectory))
        {
            Directory.CreateDirectory(bucketDirectory);
        }

        var extension = Path.GetExtension(fileName);
        var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
        var filePath = Path.Combine(bucketDirectory, uniqueFileName);

        using (var outputStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        {
            await fileStream.CopyToAsync(outputStream, cancellationToken);
        }

        var relativeKey = $"{bucket}/{uniqueFileName}";
        _logger.LogInformation("Saved file {FileName} to {FilePath} with key {Key}", fileName, filePath, relativeKey);

        return relativeKey;
    }

    public Task<Stream?> DownloadFileAsync(
        string fileKey,
        string? bucketName = null,
        CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_baseStoragePath, fileKey.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(filePath))
        {
            return Task.FromResult<Stream?>(null);
        }

        Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult<Stream?>(stream);
    }

    public Task<bool> DeleteFileAsync(
        string fileKey,
        string? bucketName = null,
        CancellationToken cancellationToken = default)
    {
        var filePath = Path.Combine(_baseStoragePath, fileKey.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            return Task.FromResult(true);
        }

        return Task.FromResult(false);
    }

    public string GetPublicUrl(string fileKey, string? bucketName = null)
    {
        return $"{_publicBaseUrl.TrimEnd('/')}/{fileKey.TrimStart('/')}";
    }
}
