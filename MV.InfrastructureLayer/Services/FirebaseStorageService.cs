using System;
using System.IO;
using System.Threading.Tasks;
using Google.Cloud.Storage.V1;
using Google.Apis.Auth.OAuth2;
using MV.ApplicationLayer.ServiceInterfaces;
using Microsoft.Extensions.Configuration;

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

        public async Task<string> UploadImageAsync(byte[] imageBytes, string fileName)
        {
            try
            {
                var stream = new MemoryStream(imageBytes);
                var objectName = $"images/{fileName}";
                await _storageClient.UploadObjectAsync(_bucketName, objectName, "image/jpeg", stream);
                return $"https://storage.googleapis.com/{_bucketName}/{objectName}";
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
                var objectName = $"images/{fileName}";
                await _storageClient.DeleteObjectAsync(_bucketName, objectName);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting image from Firebase Storage: {ex.Message}");
            }
        }
    }
} 