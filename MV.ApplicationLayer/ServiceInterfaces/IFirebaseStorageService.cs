using System.Threading.Tasks;

namespace MV.ApplicationLayer.ServiceInterfaces
{
    public interface IFirebaseStorageService
    {
        Task<string> UploadImageAsync(byte[] imageBytes, string fileName);
        Task DeleteImageAsync(string imageUrl);
        Task<string> UpdateImageAsync(byte[] imageBytes, string fileName, string oldImageUrl);
    }
} 