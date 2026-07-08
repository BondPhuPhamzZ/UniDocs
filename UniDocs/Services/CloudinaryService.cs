using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace UniDocs.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration config)
        {
            var acc = new Account(
                config["Cloudinary:CloudName"],
                config["Cloudinary:ApiKey"],
                config["Cloudinary:ApiSecret"]
            );
            _cloudinary = new Cloudinary(acc);
        }

        public async Task<(string url, string publicId)> UploadDocumentAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return (null, null);

            UploadResult uploadResult = null;

            using (var stream = file.OpenReadStream())
            {
                var extension = System.IO.Path.GetExtension(file.FileName).ToLower();

                if (extension == ".pdf")
                {
                    var uploadParams = new ImageUploadParams()
                    {
                        File = new FileDescription(file.FileName, stream),
                        Folder = "unidocs",
                    };
                    uploadResult = await _cloudinary.UploadAsync(uploadParams);
                }
                else
                {
                    var uploadParams = new RawUploadParams()
                    {
                        File = new FileDescription(file.FileName, stream),
                        Folder = "unidocs",
                    };
                    uploadResult = await _cloudinary.UploadAsync(uploadParams);
                }
            }

            return (uploadResult.SecureUrl?.ToString(), uploadResult.PublicId);
        }

        public async Task DeleteDocumentAsync(string publicId)
        {
            // Xóa các file được upload dạng Raw (Word, Powerpoint...)
            await _cloudinary.DeleteResourcesAsync(new DelResParams()
            {
                PublicIds = new List<string> { publicId },
                ResourceType = ResourceType.Raw
            });

            // Xóa các file được upload dạng Image (PDF...)
            await _cloudinary.DeleteResourcesAsync(new DelResParams()
            {
                PublicIds = new List<string> { publicId },
                ResourceType = ResourceType.Image
            });
        }
    }
}
