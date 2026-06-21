using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace UniDocs.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration config)
        {
            var account = new Account(
                config["Cloudinary:CloudName"],
                config["Cloudinary:ApiKey"],
                config["Cloudinary:ApiSecret"]
            );
            _cloudinary = new Cloudinary(account);
        }

        // Upload
        public async Task<(string url, string publicId)> UploadDocumentAsync(IFormFile file)
        {
            if (file == null || file.Length == 0) 
                return (null, null);

            using var stream = file.OpenReadStream();
            var uploadParams = new RawUploadParams()
            {
                File = new FileDescription(file.FileName, stream)
            };
            var result = await _cloudinary.UploadAsync(uploadParams);
            return (result.SecureUrl.ToString(), result.PublicId);
        }

        // Delete
        public async Task<bool> DeleteDocumentAsync(string publicId)
        {
            if (string.IsNullOrEmpty(publicId))  
                return false;

            var deleteParams = new DelResParams()
            {
                PublicIds = new List<string> { publicId },
                ResourceType = ResourceType.Raw
            };

            var result = await _cloudinary.DeleteResourcesAsync(deleteParams);
            return result.Error == null;
        }

    }
}
