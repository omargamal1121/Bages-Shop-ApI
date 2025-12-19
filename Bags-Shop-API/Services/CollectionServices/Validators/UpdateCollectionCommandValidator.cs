using Bags_Shop_API.Services.CollectionServices.Commands;
using FluentValidation;

namespace Bags_Shop_API.Services.CollectionServices.Validators
{
    public class UpdateCollectionCommandValidator : AbstractValidator<UpdateCollectionCommand>
    {
        public UpdateCollectionCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Collection ID must be greater than 0");

            RuleFor(x => x.ArName)
                .Length(3, 100).WithMessage("Arabic name must be between 3 and 100 characters")
                .When(x => !string.IsNullOrEmpty(x.ArName));

            RuleFor(x => x.EnName)
                .Length(3, 100).WithMessage("English name must be between 3 and 100 characters")
                .When(x => !string.IsNullOrEmpty(x.EnName));

            RuleFor(x => x.ArDescription)
                .Length(10, 500).WithMessage("Arabic description must be between 10 and 500 characters")
                .When(x => !string.IsNullOrEmpty(x.ArDescription));

            RuleFor(x => x.EnDescription)
                .Length(10, 500).WithMessage("English description must be between 10 and 500 characters")
                .When(x => !string.IsNullOrEmpty(x.EnDescription));
        }
    }
}
