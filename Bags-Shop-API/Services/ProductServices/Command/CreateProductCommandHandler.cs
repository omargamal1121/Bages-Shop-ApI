using Bags_Shop_API.Services.ProductServices.ProductFactories;
using Bags_Shop_API.Services.Shared;
using Bags_Shop_API.UnitOfWorkService;
using MediatR;

namespace Bags_Shop_API.Services.ProductServices.Command
{
	public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<int>>
	{
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductFactory _productFactory;
        private readonly IProductMapper _productMapper;
		public CreateProductCommandHandler(IUnitOfWork unitOfWork ,IProductFactory productFactory,IProductMapper productMapper)
		{
            _productMapper = productMapper;
			_unitOfWork = unitOfWork;
            _productFactory= productFactory;
		}
		public async Task<Result<int>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
		{
            if (request == null)
                return Result<int>.Fail("Invalid Request");
           var product=_productFactory.CreateProduct(request.EnName,request.ArName,request.EnDescription,request.ArDescription,request.Price);
            if (!product.Success || product.Data is null)
                return Result<int>.Fail(product.Message);
            await _unitOfWork.Products.AddAsync(product.Data);
            await _unitOfWork.SaveChangesAsync();
            return Result<int>.Ok(product.Data.Id);

		}
	}

}
