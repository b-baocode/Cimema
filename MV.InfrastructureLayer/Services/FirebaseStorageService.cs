using System;
using System.IO;
using System.Threading.Tasks;
using Google.Cloud.Storage.V1;
using Google.Apis.Auth.OAuth2;
using MV.ApplicationLayer.ServiceInterfaces;
using Microsoft.Extensions.Configuration;
using System.Net.Http;

namespace MV.InfrastructureLayer.Services
{
    public class FirebaseStorageService : IFirebaseStorageService
    {
        private readonly StorageClient _storageClient;
        private readonly string _bucketName;
        private readonly HttpClient _httpClient;

        public FirebaseStorageService(IConfiguration configuration)
        {
            var credentialsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "firebase-credentials.json");
            _storageClient = StorageClient.Create(GoogleCredential.FromFile(credentialsPath));
            _bucketName = configuration["Firebase:StorageBucket"] ?? throw new ArgumentNullException("Firebase:StorageBucket configuration is missing");
            _httpClient = new HttpClient();
        }

        public async Task<bool> ValidateImageUrlAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return false;

            try
            {
                var response = await _httpClient.GetAsync(imageUrl);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<string> UploadImageAsync(Stream imageStream, string fileName)
        {
            try
            {
                var objectName = $"images/{fileName}";
                await _storageClient.UploadObjectAsync(_bucketName, objectName, "image/jpeg", imageStream);
                var imageUrl = $"https://firebasestorage.googleapis.com/v0/b/{_bucketName}/o/{Uri.EscapeDataString(objectName)}?alt=media";
                
                // Validate the uploaded image URL
                if (!await ValidateImageUrlAsync(imageUrl))
                {
                    await DeleteImageAsync(imageUrl);
                    throw new Exception("Failed to validate uploaded image URL");
                }

                return imageUrl;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error uploading image to Firebase Storage: {ex.Message}");
            }
        }

        public async Task<string> UpdateImageAsync(Stream imageStream, string fileName, string oldImageUrl)
        {
            string newImageUrl = null;
            try
            {
                // Upload new image first
                newImageUrl = await UploadImageAsync(imageStream, fileName);

                // If new image upload is successful, delete the old image
                if (!string.IsNullOrEmpty(oldImageUrl))
                {
                    await DeleteImageAsync(oldImageUrl);
                }

                return newImageUrl;
            }
            catch (Exception ex)
            {
                // If new image upload failed, delete it if it was created
                if (newImageUrl != null)
                {
                    await DeleteImageAsync(newImageUrl);
                }
                throw new Exception($"Error updating image in Firebase Storage: {ex.Message}");
            }
        }

        public async Task DeleteImageAsync(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl)) return;

                // Skip deletion if URL is not a valid Firebase Storage URL
                if (!imageUrl.Contains(_bucketName))
                {
                    Console.WriteLine($"Warning: Skipping deletion of invalid Firebase Storage URL: {imageUrl}");
                    return;
                }

                // Extract object name from Firebase Storage URL
                var uri = new Uri(imageUrl);
                var pathSegments = uri.AbsolutePath.Split('/');
                var objectName = string.Join("/", pathSegments.Skip(pathSegments.Length - 2));

                try
                {
                    await _storageClient.DeleteObjectAsync(_bucketName, objectName);
                }
                catch (Exception ex)
                {
                    // Log the error but don't throw - the image might already be deleted
                    Console.WriteLine($"Warning: Error deleting image from Firebase Storage: {ex.Message}");
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't throw for invalid URLs
                Console.WriteLine($"Warning: Error processing image deletion: {ex.Message}");
            }
        }
    }
} 