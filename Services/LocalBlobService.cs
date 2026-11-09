using Microsoft.AspNetCore.Http; 
namespace RaceDayAPI.Services
{
    public class LocalBlobService : IBlobService
    {
        private readonly string _uploadDir;
        private readonly string _publicBaseUrl;

        public LocalBlobService(IWebHostEnvironment env, IConfiguration cfg)
        {
            _uploadDir = Path.Combine(env.WebRootPath ?? "wwwroot", "uploads", "banners");
            Directory.CreateDirectory(_uploadDir);

            var configured = cfg["ApiSettings:PublicBaseUrl"];
            if (string.IsNullOrWhiteSpace(configured))
                throw new InvalidOperationException(
                    "ApiSettings:PublicBaseUrl is required in appsettings.json.");

            _publicBaseUrl = configured.TrimEnd('/') + "/";
        }

        public async Task<string> UploadAsync(IFormFile file, string fileName)
        {
            var path = Path.Combine(_uploadDir, fileName);
            using var stream = new FileStream(path, FileMode.Create);
            await file.CopyToAsync(stream);
            return $"{_publicBaseUrl}{fileName}";
        }

        public Task DeleteAsync(string fileName)
        {
            var path = Path.Combine(_uploadDir, fileName);
            if (File.Exists(path)) File.Delete(path);
            return Task.CompletedTask;
        }
    }
}
