using Bags_Shop_API.Services.Shared;

namespace Bags_Shop_API.Services.ImageServices
{
    public interface IImageValidationService
    {
        bool IsValidExtension(string extension);
        bool IsValidContentType(string contentType);
        bool IsValidFileSize(long fileSize);
        bool IsValidFileSignature(Stream fileStream);
        Result<bool> ValidateImage(IFormFile image);
    }
}
