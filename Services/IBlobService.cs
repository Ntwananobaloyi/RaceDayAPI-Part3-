using Microsoft.AspNetCore.Http;
namespace RaceDayAPI.Services
{
    public interface IBlobService
    {
        Task<string> UploadAsync(IFormFile file, string fileName);
        Task DeleteAsync(string fileName);
    }
}
