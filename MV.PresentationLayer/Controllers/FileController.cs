using Microsoft.AspNetCore.Mvc;
using MV.InfrastructureLayer.Services;
using System.Threading.Tasks;

namespace MV.PresentationLayer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly IFirebaseStorageService _firebaseStorageService;

        public FileController(IFirebaseStorageService firebaseStorageService)
        {
            _firebaseStorageService = firebaseStorageService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file, [FromQuery] string folderPath)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded");

            try
            {
                string fileUrl = await _firebaseStorageService.UploadFileAsync(file, folderPath);
                return Ok(new { url = fileUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteFile([FromQuery] string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
                return BadRequest("File URL is required");

            try
            {
                await _firebaseStorageService.DeleteFileAsync(fileUrl);
                return Ok("File deleted successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("url")]
        public async Task<IActionResult> GetFileUrl([FromQuery] string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return BadRequest("File path is required");

            try
            {
                string fileUrl = await _firebaseStorageService.GetFileUrlAsync(filePath);
                return Ok(new { url = fileUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
} 