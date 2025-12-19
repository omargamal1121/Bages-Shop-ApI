using Bags_Shop_API;
using Bags_Shop_API.Services.Shared;
using MediatR;

namespace Bags_Shop_API.Services.ImageServices.Commands
{
    public class SaveImagesCommandHandler : IRequestHandler<SaveImagesCommand, Result<List<Image>>>
    {
        private readonly ILogger<SaveImagesCommandHandler> _logger;
        private readonly IMediator _mediator;

        public SaveImagesCommandHandler(
            ILogger<SaveImagesCommandHandler> logger,
            IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        public async Task<Result<List<Image>>> Handle(SaveImagesCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Saving {Count} images", request.Images?.Count);

            if (request.Images == null || request.Images.Count == 0)
                return Result<List<Image>>.Fail("Images are null or empty");

            var tasks = request.Images.Select(img => 
                _mediator.Send(new SaveImageCommand(img, request.EntityId, request.IsProduct), cancellationToken));
            var results = await Task.WhenAll(tasks);

            var savedImages = new List<Image>();
            var errors = new List<string>();

            for (int i = 0; i < results.Length; i++)
            {
                var result = results[i];
                if (!result.Success || result.Data == null)
                {
                    errors.Add($"Image #{i + 1}: {result.Message}");
                    _logger.LogError("Failed to save image #{Index}: {Message}", i + 1, result.Message);
                }
                else
                {
                    savedImages.Add(result.Data);
                }
            }

            if (errors.Any())
            {
                var warningMessage = $"Some images failed to save: {string.Join(" | ", errors)}";
                _logger.LogWarning(warningMessage);

                return new Result<List<Image>>
                {
                    Message = savedImages.Any() ? "Partial success" : "All images failed",
                    Success = savedImages.Any(),
                    Warnings = errors,
                    Data = savedImages,
                    StatusCode = savedImages.Any() ? 207 : 400
                };
            }

            return Result<List<Image>>.Ok(savedImages, $"Successfully saved {savedImages.Count} images");
        }
    }
}
