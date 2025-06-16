using System.Web;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;
using MV.ApplicationLayer.ServiceInterfaces;

namespace MV.InfrastructureLayer.Services
{
    public class FirebaseStorageService : IFirebaseStorageService
    {
        private readonly StorageClient _storageClient;
        private readonly string _bucketName;

        public FirebaseStorageService(IConfiguration configuration)
        {
            var credentialsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "firebase-credentials.json");
            _storageClient = StorageClient.Create(GoogleCredential.FromFile(credentialsPath));
            _bucketName = configuration["Firebase:StorageBucket"] ?? throw new ArgumentNullException("Firebase:StorageBucket configuration is missing");
        }

        public async Task<string> UploadImageAsync(Stream imageStream, string fileName, string folderPath)
        {
            if (imageStream == null)
            {
                throw new ArgumentNullException(nameof(imageStream));
            }
            if (string.IsNullOrWhiteSpace(fileName))
            {
                throw new ArgumentException("File name cannot be null or empty.", nameof(fileName));
            }
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                throw new ArgumentException("Folder path cannot be null or empty.", nameof(folderPath));
            }

            try
            {
                string cleanedFolderPath = folderPath.Trim('/').Replace("\\", "/");


                var objectName = $"{cleanedFolderPath}/{fileName}";

                await _storageClient.UploadObjectAsync(_bucketName, objectName, "image/jpeg", imageStream);
                return $"https://firebasestorage.googleapis.com/v0/b/{_bucketName}/o/{Uri.EscapeDataString(objectName)}?alt=media";
            }
            catch (Exception ex)
            {
                throw new Exception($"Error uploading image to Firebase Storage: {ex.Message}");
            }
        }

        public async Task<string> UpdateImageAsync(Stream imageStream, string fileName, string oldImageUrl)
        {
            try
            {
                var uri = new Uri(oldImageUrl);

                string path = HttpUtility.UrlDecode(uri.AbsolutePath);

                // xoa "/v0/b/[bucket-name]/o/"
                string[] pathParts = path.Split(new[] { "/o/" }, StringSplitOptions.None);

                string oldFileName = pathParts[1];

                // xoa parameters
                int indexOfQueryParam = oldFileName.IndexOf('?');
                if (indexOfQueryParam != -1)
                {
                    oldFileName = oldFileName.Substring(0, indexOfQueryParam);
                }

                //lay path : Images/cat.jpg
                string objectPath = oldFileName;

                string oldImageFolderName = objectPath.Split('/')[0];

                // Delete old image if exists
                if (!string.IsNullOrEmpty(oldImageUrl))
                {
                    await DeleteImageAsync(oldImageUrl);
                }

                // Upload new image
                return await UploadImageAsync(imageStream, fileName, oldImageFolderName);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating image in Firebase Storage: {ex.Message}");
            }
        }

        public async Task DeleteImageAsync(string imageUrl)
        {
            try
            {
                if (imageUrl == null || imageUrl == "https://firebasestorage.googleapis.com/v0/b/swp391-2004.appspot.com/o/UserImages%2FPlaceholder-Profile-Image.jpg?alt=media&token=11cc28fe-2437-4527-a755-909c0a332ffa" ||
                    imageUrl == "https://firebasestorage.googleapis.com/v0/b/swp391-2004.appspot.com/o/UserImages%2FPlaceholder-Profile-Image.jpg?alt=media")
                {
                    return;
                }

                var uri = new Uri(imageUrl);

                string path = HttpUtility.UrlDecode(uri.AbsolutePath);

                // xoa "/v0/b/[bucket-name]/o/"
                string[] pathParts = path.Split(new[] { "/o/" }, StringSplitOptions.None);

                string fileName = pathParts[1];

                // xoa parameters
                int indexOfQueryParam = fileName.IndexOf('?');
                if (indexOfQueryParam != -1)
                {
                    fileName = fileName.Substring(0, indexOfQueryParam);
                }

                //lay path : Images/cat.jpg
                string objectPath = fileName;

                //delete
                await _storageClient.DeleteObjectAsync(_bucketName, objectPath);
            }
            catch (Exception ex)
            {

                throw new Exception($"An error occurred while deleting the image: {ex.Message}", ex);
            }
        }
    }
}