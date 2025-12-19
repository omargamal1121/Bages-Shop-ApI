using Bags_Shop_API.Services.Shared;

namespace Bags_Shop_API.Services.ImageServices
{
    public class ImageValidationService : IImageValidationService
    {
        private readonly ILogger<ImageValidationService> _logger;
        private readonly IConfiguration _configuration;

        private int MaxFileSize => _configuration.GetValue<int>("Security:FileUpload:MaxFileSizeMB", 5) * 1024 * 1024;
        private string[] AllowedContentTypes => _configuration.GetSection("Security:FileUpload:AllowedContentTypes").Get<string[]>()
            ?? new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
        private string[] AllowedExtensions => _configuration.GetSection("Security:FileUpload:AllowedExtensions").Get<string[]>()
            ?? new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        private readonly byte[][] _fileSignatures = {
            new byte[] { 0xFF, 0xD8, 0xFF },      // JPEG
            new byte[] { 0x89, 0x50, 0x4E, 0x47 }, // PNG
            new byte[] { 0x47, 0x49, 0x46, 0x38 }, // GIF
            new byte[] { 0x52, 0x49, 0x46, 0x46 }, // WEBP
        };

        public ImageValidationService(ILogger<ImageValidationService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public bool IsValidExtension(string extension) => AllowedExtensions.Contains(extension.ToLower());

        public bool IsValidContentType(string contentType) => AllowedContentTypes.Contains(contentType.ToLower());

        public bool IsValidFileSize(long fileSize) => fileSize > 0 && fileSize <= MaxFileSize;

        public bool IsValidFileSignature(Stream fileStream)
        {
            try
            {
                using var reader = new BinaryReader(fileStream, System.Text.Encoding.UTF8, true);
                var headerBytes = reader.ReadBytes(8);
                return _fileSignatures.Any(sig => headerBytes.Take(sig.Length).SequenceEqual(sig));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating file signature");
                return false;
            }
        }

        public Result<bool> ValidateImage(IFormFile image)
        {
            if (image == null)
                return Result<bool>.Fail("Image is null");

            // Validate file size
            if (!IsValidFileSize(image.Length))
                return Result<bool>.Fail($"File size must be between 1 and {MaxFileSize / (1024 * 1024)}MB");

            // Validate content type
            if (!IsValidContentType(image.ContentType))
                return Result<bool>.Fail($"Invalid content type: {image.ContentType}");

            // Validate extension
            string extension = Path.GetExtension(image.FileName);
            if (!IsValidExtension(extension))
                return Result<bool>.Fail($"Invalid extension. Allowed: {string.Join(", ", AllowedExtensions)}");

            // Validate file signature
            try
            {
                using var stream = image.OpenReadStream();
                if (!IsValidFileSignature(stream))
                    return Result<bool>.Fail("Invalid file format detected");

                return Result<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating image");
                return Result<bool>.Fail("Error validating image");
            }
        }
    }
}
