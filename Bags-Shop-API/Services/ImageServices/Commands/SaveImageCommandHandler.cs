using Bags_Shop_API;
using Bags_Shop_API.Services.Shared;
using Bags_Shop_API.UnitOfWorkService;
using MediatR;

namespace Bags_Shop_API.Services.ImageServices.Commands
{
    public class SaveImageCommandHandler : IRequestHandler<SaveImageCommand, Result<Image>>
    {
        private readonly ILogger<SaveImageCommandHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IImageValidationService _validationService;
        private readonly ICloudinaryImageService _cloudinaryService;

        public SaveImageCommandHandler(
            ILogger<SaveImageCommandHandler> logger,
            IUnitOfWork unitOfWork,
            IImageValidationService validationService,
            ICloudinaryImageService cloudinaryService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _validationService = validationService;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<Result<Image>> Handle(SaveImageCommand request, CancellationToken cancellationToken)
        {
      
            var validationResult = _validationService.ValidateImage(request.Image);
            if (!validationResult.Success)
                return Result<Image>.Fail(validationResult.Message);

            try
            {
                // Upload to Cloudinary
                var uploadResult = await _cloudinaryService.UploadImageAsync(request.Image);
                if (!uploadResult.Success || uploadResult.Data.Url == null)
                    return Result<Image>.Fail(uploadResult.Message, uploadResult.StatusCode);

                var savedImage = new Image
                {
                    CloudinaryPublicId = uploadResult.Data.PublicId,
                    ImageUrl = uploadResult.Data.Url,
                };

                if (request.IsProduct)
                    savedImage.ProductId = request.EntityId;
                else
                    savedImage.CollectionId = request.EntityId;

                // Save to database
                var imageRepo = _unitOfWork.Images;
                await imageRepo.AddAsync(savedImage);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Image saved successfully: {Url}", savedImage.ImageUrl);
                return Result<Image>.Ok(savedImage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving image");
                return Result<Image>.Fail($"Error saving image: {ex.Message}", 500);
            }
        }
    }
}
