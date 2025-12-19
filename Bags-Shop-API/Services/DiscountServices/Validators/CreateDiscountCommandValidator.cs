using Bags_Shop_API.Services.DiscountServices.Commands;
using FluentValidation;

namespace Bags_Shop_API.Services.DiscountServices.Validators
{
    public class CreateDiscountCommandValidator : AbstractValidator<CreateDiscountCommand>
    {
        public CreateDiscountCommandValidator()
        {
            RuleFor(x => x.DiscountPercentage)
                .GreaterThan(0).WithMessage("Discount percentage must be greater than 0")
                .LessThan(90).WithMessage("Discount percentage must be less than 90");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Start date is required")
                .LessThan(x => x.EndDate).WithMessage("Start date must be before end date");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("End date is required")
                .GreaterThan(DateTime.Now).WithMessage("End date must be in the future");
        }
    }
}
