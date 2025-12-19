using Bags_Shop_API.Services.DiscountServices.Commands;
using FluentValidation;

namespace Bags_Shop_API.Services.DiscountServices.Validators
{
    public class UpdateDiscountCommandValidator : AbstractValidator<UpdateDiscountCommand>
    {
        public UpdateDiscountCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Discount ID must be greater than 0");

            RuleFor(x => x.DiscountPercentage)
                .GreaterThan(0).WithMessage("Discount percentage must be greater than 0")
                .LessThan(90).WithMessage("Discount percentage must be less than 90")
                .When(x => x.DiscountPercentage.HasValue);

            RuleFor(x => x.StartDate)
                .LessThan(x => x.EndDate ?? DateTime.MaxValue).WithMessage("Start date must be before end date")
                .When(x => x.StartDate.HasValue);

            RuleFor(x => x.EndDate)
                .GreaterThan(x => x.StartDate ?? DateTime.MinValue).WithMessage("End date must be after start date")
                .When(x => x.EndDate.HasValue);
        }
    }
}
