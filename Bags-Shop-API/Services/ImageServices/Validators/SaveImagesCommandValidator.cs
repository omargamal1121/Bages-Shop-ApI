using Bags_Shop_API.Services.ImageServices.Commands;
using FluentValidation;

namespace Bags_Shop_API.Services.ImageServices.Validators
{
    public class SaveImagesCommandValidator : AbstractValidator<SaveImagesCommand>
    {
        public SaveImagesCommandValidator()
        {
            RuleFor(x => x.Images)
                .NotNull().WithMessage("Images list is required")
                .NotEmpty().WithMessage("At least one image is required");

            RuleFor(x => x.EntityId)
                .GreaterThan(0).WithMessage("Entity ID must be greater than 0");
        }
    }
}
