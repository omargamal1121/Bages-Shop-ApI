using Bags_Shop_API.Models;
using Bags_Shop_API.Services.ProductServices.ProductsDtos;
using Bags_Shop_API.Services.Shared;
using Bags_Shop_API.Specification;
using Bags_Shop_API.UnitOfWorkService;
using Hangfire;
using MediatR;
using System.Threading.Tasks;

namespace Bags_Shop_API.Services.OrderServices.Commands
{
    public class CreateOrderCommandHandler
    : IRequestHandler<CreateOrderCommand, Result<int>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBackgroundJobClient _backgroundJobClient;
        public CreateOrderCommandHandler(IUnitOfWork unitOfWork, IBackgroundJobClient backgroundJobClient)
        {
            _backgroundJobClient = backgroundJobClient;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<int>> Handle(
            CreateOrderCommand request,
            CancellationToken cancellationToken)
        {

            if (request.Items == null || !request.Items.Any())
                return Result<int>.Fail("Order must contain at least one product.", 400,null);

      
            var productIds = request.Items.Select(i => i.ProductId).ToList();

            var spec = new BaseSpecificationWithProjection<Product, ProductDto>(p=>new ProductDto 
            {
                Price = p.Price,
                Id=p.Id,
                Discount=p.Discount !=null? new DiscountServices.DiscountDto 
                {
                    Id=p.Discount.Id,
                    DiscountPercentage =p.Discount.DiscountPercentage,
                    EndDate=p.Discount.EndDate,
                    IsActive=p.Discount.IsActive,
                    StartDate = p.Discount.StartDate
                }:null
               
            });
            spec.Criteria = p => productIds.Contains(p.Id);



            var products = await _unitOfWork.Products.GetAllAsync(spec);

            if (products.Count != productIds.Count)
                return Result<int>.Fail("Some products do not exist.", 404,null);

   
            var order = new Order
            {
                Address = request.Address,
                Phone = request.Phone,
                Status = OrderStatus.Pending,
                OrderItems = new List<OrderItem>()
                ,
                
            };

            var itemsDict = request.Items.ToDictionary(x => x.ProductId);

            foreach (var product in products)
            {
                var item = itemsDict[product.Id];

                if (item.Quntity <= 0)
                    return Result<int>.Fail("Invalid quantity.");
                var orderitem = new OrderItem
                {
                    ProductId = product.Id,
                    Quantity = item.Quntity,
                    UnitPrice = (product.Discount != null && product.Discount.IsActive && product.Discount.EndDate > DateTime.UtcNow)
                    ? product.Price - (product.Price * product.Discount.DiscountPercentage / 100) : product.Price
                };
                orderitem.TotalPrice=orderitem.UnitPrice*orderitem.Quantity;
                order.OrderItems.Add(orderitem);

               

            }
            order.FinalPrice=order.OrderItems.Sum(x => x.TotalPrice);

            await _unitOfWork.Orders.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();
            _backgroundJobClient.Schedule(() => CheckOnOrder(order.Id), DateTime.UtcNow.AddHours(2));

            return Result<int>.Ok(order.Id,statusCode: 201);
        }
        public async Task CheckOnOrder(int orderid)
        {
            try
            {
                var order = await _unitOfWork.Orders.GetByIdAsync(orderid);
                if (order == null) return;
                if (order.Status == OrderStatus.Pending && order.ExpiresAt < DateTime.UtcNow)
                {
                    order.Status = OrderStatus.Expired;

                }
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }


        }

    }
  

}
