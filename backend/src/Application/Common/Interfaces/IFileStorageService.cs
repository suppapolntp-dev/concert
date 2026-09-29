namespace TicketWorld.Application.Common.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        string? bucketName = null,
        CancellationToken cancellationToken = default);

    Task<Stream?> DownloadFileAsync(
        string fileKey,
        string? bucketName = null,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteFileAsync(
        string fileKey,
        string? bucketName = null,
        CancellationToken cancellationToken = default);

    string GetPublicUrl(string fileKey, string? bucketName = null);
}
