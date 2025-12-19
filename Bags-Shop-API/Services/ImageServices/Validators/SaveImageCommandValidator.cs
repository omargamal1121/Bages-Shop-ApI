using Bags_Shop_API.Services.ImageServices.Commands;
using FluentValidation;

namespace Bags_Shop_API.Services.ImageServices.Validators
{
    public class SaveImageCommandValidator : AbstractValidator<SaveImageCommand>
    {
        public SaveImageCommandValidator()
        {
            RuleFor(x => x.Image)
                .NotNull().WithMessage("Image file is required");

            RuleFor(x => x.EntityId)
                .GreaterThan(0).WithMessage("Entity ID must be greater than 0");
        }
    }
}
