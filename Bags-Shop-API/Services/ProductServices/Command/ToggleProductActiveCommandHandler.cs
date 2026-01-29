using Bags_Shop_API.Services.ProductServices.ProductsDtos;
using Bags_Shop_API.Services.Shared;
using Bags_Shop_API.UnitOfWorkService;
using MediatR;

namespace Bags_Shop_API.Services.ProductServices.Command
{
    public class ToggleProductActiveCommandHandler : IRequestHandler<ToggleProductActiveCommand, Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductMapper _productMapper;

        public ToggleProductActiveCommandHandler(IUnitOfWork unitOfWork, IProductMapper productMapper)
        {
            _unitOfWork = unitOfWork;
            _productMapper = productMapper;
        }

        public async Task<Result<bool>> Handle(ToggleProductActiveCommand request, CancellationToken cancellationToken)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(request.Id);

            if (product == null||product.Delete_AT!=null)
                return Result<bool>.Fail($"No Product With Id {request.Id}", 404);
            if(product.Price<=0)
                return Result<bool>.Fail($"Must price More than 0", 400);


            product.IsActive = request.IsActive;
            await _unitOfWork.SaveChangesAsync();

            return Result<bool>.Ok(true);
        }
    }
}
