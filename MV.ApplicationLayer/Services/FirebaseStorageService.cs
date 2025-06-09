using System;
using System.IO;
using System.Threading.Tasks;
using Google.Cloud.Storage.V1;
using Google.Apis.Auth.OAuth2;
using MV.ApplicationLayer.ServiceInterfaces;

namespace MV.ApplicationLayer.Services
{
    public class FirebaseStorageService : IFirebaseStorageService
    {
        private readonly StorageClient _storageClient;
        private const string BucketName = "swp391-2004";

        public FirebaseStorageService()
        {
            var credentialsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "firebase-credentials.json");
            _storageClient = StorageClient.Create(GoogleCredential.FromFile(credentialsPath));
        }

        public async Task<string> UploadImageAsync(byte[] imageBytes, string fileName)
        {
            try
            {
                var stream = new MemoryStream(imageBytes);
                var objectName = $"movie-posters/{fileName}";
                await _storageClient.UploadObjectAsync(BucketName, objectName, "image/jpeg", stream);
                return $"https://storage.googleapis.com/{BucketName}/{objectName}";
            }
            catch (Exception ex)
            {
                throw new Exception($"Error uploading image to Firebase Storage: {ex.Message}");
            }
        }

        public async Task<string> UpdateImageAsync(byte[] imageBytes, string fileName, string oldImageUrl)
        {
            try
            {
                // Delete old image if exists
                if (!string.IsNullOrEmpty(oldImageUrl))
                {
                    await DeleteImageAsync(oldImageUrl);
                }

                // Upload new image
                return await UploadImageAsync(imageBytes, fileName);
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
                if (string.IsNullOrEmpty(imageUrl)) return;

                var fileName = Path.GetFileName(new Uri(imageUrl).LocalPath);
                var objectName = $"movie-posters/{fileName}";
                await _storageClient.DeleteObjectAsync(BucketName, objectName);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting image from Firebase Storage: {ex.Message}");
            }
        }
    }
} 