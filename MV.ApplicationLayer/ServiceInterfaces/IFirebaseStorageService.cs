using System.IO;
using System.Threading.Tasks;

namespace MV.ApplicationLayer.ServiceInterfaces
{
    public interface IFirebaseStorageService
    {
        Task<string> UploadImageAsync(Stream imageStream, string fileName, string folderPath);
        Task DeleteImageAsync(string imageUrl);
        Task<string> UpdateImageAsync(Stream imageStream, string fileName, string oldImageUrl);
    }
} 