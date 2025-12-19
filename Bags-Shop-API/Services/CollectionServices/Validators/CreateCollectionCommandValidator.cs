using Bags_Shop_API.Services.CollectionServices.Commands;
using FluentValidation;

namespace Bags_Shop_API.Services.CollectionServices.Validators
{
    public class CreateCollectionCommandValidator : AbstractValidator<CreateCollectionCommand>
    {
        public CreateCollectionCommandValidator()
        {
            RuleFor(x => x.ArName)
                .NotEmpty().WithMessage("Arabic name is required")
                .Length(3, 100).WithMessage("Arabic name must be between 3 and 100 characters");

            RuleFor(x => x.EnName)
                .NotEmpty().WithMessage("English name is required")
                .Length(3, 100).WithMessage("English name must be between 3 and 100 characters");

            RuleFor(x => x.ArDescription)
                .NotEmpty().WithMessage("Arabic description is required")
                .Length(10, 500).WithMessage("Arabic description must be between 10 and 500 characters");

            RuleFor(x => x.EnDescription)
                .NotEmpty().WithMessage("English description is required")
                .Length(10, 500).WithMessage("English description must be between 10 and 500 characters");
        }
    }
}
