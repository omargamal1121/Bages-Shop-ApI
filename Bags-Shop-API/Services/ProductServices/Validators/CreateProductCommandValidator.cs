using Bags_Shop_API.Services.ProductServices.Command;
using FluentValidation;

namespace Bags_Shop_API.Services.ProductServices.Validators
{
    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
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
