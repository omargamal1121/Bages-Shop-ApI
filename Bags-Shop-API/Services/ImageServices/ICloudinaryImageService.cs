using Bags_Shop_API.Services.Shared;

namespace Bags_Shop_API.Services.ImageServices
{
    public interface ICloudinaryImageService
    {
        Task<Result<(string Url, string PublicId)>> UploadImageAsync(IFormFile image);
        void EnqueueDeletion(string publicId);
        Task DeleteFromCloudinaryAsync(string publicId);
    }
}
