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

        public async Task<string> UploadImageAsync(Stream imageStream, string fileName)
        {
            try
            {
                var objectName = $"images/{fileName}";
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
                // Delete old image if exists
                if (!string.IsNullOrEmpty(oldImageUrl))
                {
                    await DeleteImageAsync(oldImageUrl);
                }

                // Upload new image
                return await UploadImageAsync(imageStream, fileName);
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

                // Extract object name from Firebase Storage URL
                var uri = new Uri(imageUrl);
                var pathSegments = uri.AbsolutePath.Split('/');
                var objectName = string.Join("/", pathSegments.Skip(pathSegments.Length - 2));
                await _storageClient.DeleteObjectAsync(_bucketName, objectName);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting image from Firebase Storage: {ex.Message}");
            }
        }
    }
} 