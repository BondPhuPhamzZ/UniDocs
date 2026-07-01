using Firebase.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace UniDocs.Services
{
    public class FirebaseStorageService : IFirebaseStorageService
    {
        private readonly string _bucket;

        public FirebaseStorageService(IConfiguration config)
        {
            _bucket = config["Firebase:Bucket"] ?? "your-app.appspot.com";
        }

        public async Task<string> UploadFileAsync(IFormFile file, string folderName)
        {
            if (file == null || file.Length == 0)
                return null;

            var stream = file.OpenReadStream();
            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";

            var task = new FirebaseStorage(_bucket)
                .Child(folderName)
                .Child(fileName)
                .PutAsync(stream);

            var downloadUrl = await task;
            return downloadUrl;
        }

        public async Task DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl)) return;

            try
            {
                // fileUrl example: https://firebasestorage.googleapis.com/v0/b/bucket/o/folderName%2FfileName?alt=media...
                Uri uri = new Uri(fileUrl);
                string path = uri.AbsolutePath; 
                // path looks like /v0/b/bucket/o/folderName%2FfileName
                string prefix = $"/v0/b/{_bucket}/o/";
                
                if (path.StartsWith(prefix))
                {
                    string objectPath = path.Substring(prefix.Length);
                    // Decode %2F to /
                    objectPath = Uri.UnescapeDataString(objectPath);

                    string[] segments = objectPath.Split('/');
                    
                    var storageRef = new FirebaseStorage(_bucket);
                    FirebaseStorageReference childRef = null;
                    
                    foreach (var segment in segments)
                    {
                        if (childRef == null)
                            childRef = storageRef.Child(segment);
                        else
                            childRef = childRef.Child(segment);
                    }

                    if (childRef != null)
                    {
                        await childRef.DeleteAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting from Firebase: {ex.Message}");
            }
        }
    }
}
