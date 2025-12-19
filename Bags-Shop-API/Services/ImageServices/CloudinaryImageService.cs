using Bags_Shop_API.Services.Shared;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Hangfire;

namespace Bags_Shop_API.Services.ImageServices
{
    public class CloudinaryImageService : ICloudinaryImageService
    {
        private readonly ILogger<CloudinaryImageService> _logger;
        private readonly Cloudinary _cloudinary;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public CloudinaryImageService(
            ILogger<CloudinaryImageService> logger,
            Cloudinary cloudinary,
            IBackgroundJobClient backgroundJobClient)
        {
            _logger = logger;
            _cloudinary = cloudinary;
            _backgroundJobClient = backgroundJobClient;
        }

        public async Task<Result<(string Url, string PublicId)>> UploadImageAsync(IFormFile image)
        {
            try
            {
                using var stream = image.OpenReadStream();
                stream.Position = 0;

                var publicId = Guid.NewGuid().ToString();
                var uploadParams = new ImageUploadParams
                {
                    File = new CloudinaryDotNet.FileDescription(image.FileName, stream),
                    PublicId = publicId,
                    Overwrite = false,
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult?.SecureUrl == null || uploadResult.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    _logger.LogError("Cloudinary upload failed: {Error}", uploadResult?.Error?.Message);
                    return Result<(string, string)>.Fail("Failed to upload image to Cloudinary");
                }

                _logger.LogInformation("Image uploaded successfully to Cloudinary: {Url}", uploadResult.Url.AbsoluteUri);
                return Result<(string, string)>.Ok((uploadResult.Url.AbsoluteUri, uploadResult.PublicId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading image to Cloudinary");
                return Result<(string, string)>.Fail($"Error uploading image: {ex.Message}", 500);
            }
        }

        public void EnqueueDeletion(string publicId)
        {
            if (!string.IsNullOrEmpty(publicId))
            {
                _backgroundJobClient.Enqueue(() => DeleteFromCloudinaryAsync(publicId));
            }
        }

        public async Task DeleteFromCloudinaryAsync(string publicId)
        {
            try
            {
                var deletionParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(deletionParams);

                if (result.Result == "ok")
                {
                    _logger.LogInformation("Successfully deleted from Cloudinary: {PublicId}", publicId);
                }
                else
                {
                    _logger.LogWarning("Cloudinary deletion returned: {Result} for {PublicId}", result.Result, publicId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete from Cloudinary: {PublicId}", publicId);
            }
        }
    }
}
