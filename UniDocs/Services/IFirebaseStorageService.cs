namespace UniDocs.Services
{
    public interface IFirebaseStorageService
    {
        Task<string> UploadFileAsync(IFormFile file, string folderName);
        Task DeleteFileAsync(string fileUrl);
    }
}
