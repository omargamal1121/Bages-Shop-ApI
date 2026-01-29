using Bags_Shop_API.Services.ImageServices.Commands;
using Bags_Shop_API.Services.ProductServices.ProductsDtos;
using Bags_Shop_API.Services.Shared;
using Bags_Shop_API.UnitOfWorkService;
using MediatR;

namespace Bags_Shop_API.Services.ProductServices.Command
{
	public class UpdateProductCommandHandler
        : IRequestHandler<UpdateProductCommand, Result<ProductDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductMapper _productMapper;
        private readonly IMediator _mediator;

        public UpdateProductCommandHandler(IUnitOfWork unitOfWork, IProductMapper productMapper, IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _productMapper = productMapper;
            _mediator = mediator;
        }

        public async Task<Result<ProductDto>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(request.Id);

            if (product == null||product.Delete_AT is not null)
                return Result<ProductDto>.Fail($"No Product With Id {request.Id}", 404);

            bool isUpdated = false;

            if (!string.IsNullOrEmpty(request.EnName))
            {
                product.EnName = request.EnName;
                isUpdated = true;
            }

            if (!string.IsNullOrEmpty(request.ArName))
            {
                product.ArName = request.ArName;
                isUpdated = true;
            }

            if (!string.IsNullOrEmpty(request.ArDescription))
            {
                product.ArDescription = request.ArDescription;
                isUpdated = true;
            }

            if (!string.IsNullOrEmpty(request.EnDescription))
            {
                product.EnDescription = request.EnDescription;
                isUpdated = true;
            }
            if(request.Price.HasValue)
            {
                product.Price = request.Price.Value;
                isUpdated = true;
            }

        
            

            if (!isUpdated)
                return Result<ProductDto>.Fail("No valid fields to update");

            await _unitOfWork.SaveChangesAsync();

            return Result<ProductDto>.Ok(_productMapper.UpdateProductDto(product));
        }
    }

}


