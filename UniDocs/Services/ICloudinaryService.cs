namespace UniDocs.Services
{
    public interface ICloudinaryService
    {
        Task<(string url, string publicId)> UploadDocumentAsync(IFormFile file);
        Task<bool> DeleteDocumentAsync(string publicId);
    }
}
