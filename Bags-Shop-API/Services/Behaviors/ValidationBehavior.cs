using Bags_Shop_API.Services.Shared;
using FluentValidation;
using MediatR;

namespace Bags_Shop_API.Services.Behaviors
{
    public class ValidationBehavior<TRequest, T>
     : IPipelineBehavior<TRequest, Result<T>>
     where TRequest : IRequest<Result<T>>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<Result<T>> Handle(
            TRequest request,
            RequestHandlerDelegate<Result<T>> next,
            CancellationToken cancellationToken)
        {
            if (!_validators.Any())
                return await next();

            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken))
            );

            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Any())
            {
                var errorMessage = string.Join("; ", failures.Select(f => f.ErrorMessage));
                return Result<T>.Fail(errorMessage, 400);
            }

            return await next();
        }
    }

}
