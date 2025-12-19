using Bags_Shop_API.Services.Shared;
using MediatR;

namespace Bags_Shop_API.Services.Behaviors
{
	public class CacheInvalidationBehavior<TRequest, TResponse>
      : IPipelineBehavior<TRequest, TResponse>
      where TRequest : IRequest<TResponse>
    {
        private readonly ICacheTokenProvider _tokenProvider;

        public CacheInvalidationBehavior(ICacheTokenProvider tokenProvider)
        {
            _tokenProvider = tokenProvider;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var response = await next();

            if (request is IInvalidateCache)
            {
                if (response is Shared.IResult result && result.Success)
                {
                    _tokenProvider.Reset();
                }
                else if (response is not Shared.IResult) 
                {
                     // If it doesn't implement IResult, assume success or just invalidate anyway?
                     _tokenProvider.Reset();
                }
            }

            return response;
        }
    }
}
