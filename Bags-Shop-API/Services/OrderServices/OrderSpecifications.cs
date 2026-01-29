using Bags_Shop_API.Models;
using Bags_Shop_API.Services.OrderServices.Dtos;
using Bags_Shop_API.Specification;

namespace Bags_Shop_API.Services.OrderServices
{
    public class OrderWithItemsProjectionSpec : BaseSpecificationWithProjection<Order, OrderDto>
    {
        public OrderWithItemsProjectionSpec() : base(o => new OrderDto
        {
            Id = o.Id,
            Address = o.Address,
            Phone = o.Phone,
            Status = o.Status.ToString(),
            FinalPrice = o.FinalPrice,
            CreatedAt = o.CreatedAt,
            ExpiresAt = o.ExpiresAt,
            Name = o.Name ?? "",
            OrderItems = o.OrderItems.Select(oi => new OrderItemSummaryDto
            {
                ProductId = oi.ProductId,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                TotalPrice = oi.TotalPrice,
                Product = oi.Product != null ? new OrderProductDto
                {
                    Id = oi.Product.Id,
                    ArName = oi.Product.ArName,
                    EnName = oi.Product.EnName,
                    ArDescription = oi.Product.ArDescription,
                    EnDescription = oi.Product.EnDescription,
                    Images = oi.Product.Images.Select(img => new OrderProductImageDto
                    {
                        Id = img.Id,
                        ImageUrl = img.ImageUrl,
                        CloudinaryPublicId = img.CloudinaryPublicId
                    }).ToList()
                } : null
            }).ToList(),
            Payments = o.Payments != null ? o.Payments.Select(p => new PaymentSummaryDto
            {
                Id = p.Id,
                Amount = p.Amount,
                Currency = p.Currency,
                Method = p.Method.ToString(),
                Status = p.Status.ToString(),
                TransactionId = p.TransactionId,
                CreatedAt = p.CreatedAt,
                PaymentLink = p.PaymentLink,
                PaymentIntentionId = p.PaymentIntentionId,
                PaymentLinkExpiresAt = p.PaymentLinkExpiresAt
            }).ToList() : new List<PaymentSummaryDto>()
        })
        {
        }

        public OrderWithItemsProjectionSpec(int id) : base(
            o => o.Id == id,
            o => new OrderDto
            {
                Id = o.Id,
                Address = o.Address,
                Phone = o.Phone,
                Status = o.Status.ToString(),
                FinalPrice = o.FinalPrice,
                CreatedAt = o.CreatedAt,
                ExpiresAt = o.ExpiresAt,
                Name = o.Name ?? "",
                OrderItems = o.OrderItems.Select(oi => new OrderItemSummaryDto
                {
                    ProductId = oi.ProductId,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.TotalPrice,
                    Product = oi.Product != null ? new OrderProductDto
                    {
                        Id = oi.Product.Id,
                        ArName = oi.Product.ArName,
                        EnName = oi.Product.EnName,
                        ArDescription = oi.Product.ArDescription,
                        EnDescription = oi.Product.EnDescription,
                        Images = oi.Product.Images.Select(img => new OrderProductImageDto
                        {
                            Id = img.Id,
                            ImageUrl = img.ImageUrl,
                            CloudinaryPublicId = img.CloudinaryPublicId
                        }).ToList()
                    } : null
                }).ToList(),
                Payments = o.Payments != null ? o.Payments.Select(p => new PaymentSummaryDto
                {
                    Id = p.Id,
                    Amount = p.Amount,
                    Currency = p.Currency,
                    Method = p.Method.ToString(),
                    Status = p.Status.ToString(),
                    TransactionId = p.TransactionId,
                    CreatedAt = p.CreatedAt,
                    PaymentLink = p.PaymentLink,
                    PaymentIntentionId = p.PaymentIntentionId,
                    PaymentLinkExpiresAt = p.PaymentLinkExpiresAt
                }).ToList() : new List<PaymentSummaryDto>()
            })
        {
        }
    }

    public class OrderWithFiltersProjectionSpec : BaseSpecificationWithProjection<Order, OrderDto>
    {
        public OrderWithFiltersProjectionSpec(DateTime? createdFrom, DateTime? createdTo, OrderStatus? status, int page, int pageSize)
            : base(
                o => (!createdFrom.HasValue || o.CreatedAt >= createdFrom.Value) &&
                     (!createdTo.HasValue || o.CreatedAt <= createdTo.Value) &&
                     (!status.HasValue || o.Status == status.Value),
                o => new OrderDto
                {
                    Id = o.Id,
                    Address = o.Address,
                    Phone = o.Phone,
                    Status = o.Status.ToString(),
                    FinalPrice = o.FinalPrice,
                    CreatedAt = o.CreatedAt,
                    ExpiresAt = o.ExpiresAt,
                    Userkey= o.Userkey,
                    Name = o.Name ?? "",
                    OrderItems = o.OrderItems.Select(oi => new OrderItemSummaryDto
                    {
                        ProductId = oi.ProductId,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.TotalPrice,
                        Product = oi.Product != null ? new OrderProductDto
                        {
                            Id = oi.Product.Id,
                            ArName = oi.Product.ArName,
                            EnName = oi.Product.EnName,
                            ArDescription = oi.Product.ArDescription,
                            EnDescription = oi.Product.EnDescription,
                            Images = oi.Product.Images.Select(img => new OrderProductImageDto
                            {
                                Id = img.Id,
                                ImageUrl = img.ImageUrl,
                                CloudinaryPublicId = img.CloudinaryPublicId
                            }).ToList()
                        } : null
                    }).ToList(),
                    Payments = o.Payments != null ? o.Payments.Select(p => new PaymentSummaryDto
                    {
                        Id = p.Id,
                        Amount = p.Amount,
                        Currency = p.Currency,
                        Method = p.Method.ToString(),
                        Status = p.Status.ToString(),
                        TransactionId = p.TransactionId,
                        CreatedAt = p.CreatedAt,
                        PaymentLink = p.PaymentLink,
                        PaymentIntentionId = p.PaymentIntentionId,
                        PaymentLinkExpiresAt = p.PaymentLinkExpiresAt
                    }).ToList() : new List<PaymentSummaryDto>()
                })
        {
            ApplyOrderByDescending(o => o.CreatedAt);
            ApplyPaging(page, pageSize);
        }
    }

    public class OrdersByUserKeyProjectionSpec : BaseSpecificationWithProjection<Order, OrderDto>
    {
        public OrdersByUserKeyProjectionSpec(string userKey, DateTime? createdFrom, DateTime? createdTo, OrderStatus? status, int page, int pageSize)
            : base(
                o => o.Userkey == userKey &&
                     (!createdFrom.HasValue || o.CreatedAt >= createdFrom.Value) &&
                     (!createdTo.HasValue || o.CreatedAt <= createdTo.Value) &&
                     (!status.HasValue || o.Status == status.Value),
                o => new OrderDto
                {
                    Id = o.Id,
                    Address = o.Address,
                    Name = o.Name??"",
                    Phone = o.Phone,
                    Status = o.Status.ToString(),
                    FinalPrice = o.FinalPrice,
                    CreatedAt = o.CreatedAt,
                    ExpiresAt = o.ExpiresAt,
                    
                    OrderItems = o.OrderItems.Select(oi => new OrderItemSummaryDto
                    {
                        ProductId = oi.ProductId,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.TotalPrice,
                        Product = oi.Product != null ? new OrderProductDto
                        {
                            Id = oi.Product.Id,
                            ArName = oi.Product.ArName,
                            EnName = oi.Product.EnName,
                            ArDescription = oi.Product.ArDescription,
                            EnDescription = oi.Product.EnDescription,
                            Images = oi.Product.Images.Select(img => new OrderProductImageDto
                            {
                                Id = img.Id,
                                ImageUrl = img.ImageUrl,
                                CloudinaryPublicId = img.CloudinaryPublicId
                            }).ToList()
                        } : null
                    }).ToList(),
                    Payments = o.Payments != null ? o.Payments.Select(p => new PaymentSummaryDto
                    {
                        Id = p.Id,
                        Amount = p.Amount,
                        Currency = p.Currency,
                        Method = p.Method.ToString(),
                        Status = p.Status.ToString(),
                        TransactionId = p.TransactionId,
                        CreatedAt = p.CreatedAt,
                        PaymentLink = p.PaymentLink,
                        PaymentIntentionId = p.PaymentIntentionId,
                        PaymentLinkExpiresAt = p.PaymentLinkExpiresAt
                    }).ToList() : new List<PaymentSummaryDto>()
                })
        {
            ApplyOrderByDescending(o => o.CreatedAt);
            ApplyPaging(page, pageSize);
        }
    }

    public class OrderByIdAndUserKeyProjectionSpec : BaseSpecificationWithProjection<Order, OrderDto>
    {
        public OrderByIdAndUserKeyProjectionSpec(int id, string userKey)
            : base(
                o => o.Id == id && o.Userkey == userKey,
                o => new OrderDto
                {
                    Id = o.Id,
                    Address = o.Address,
                    Phone = o.Phone,
                    Status = o.Status.ToString(),
                    Name = o.Name??"",
                    FinalPrice = o.FinalPrice,
                    CreatedAt = o.CreatedAt,
                    ExpiresAt = o.ExpiresAt,
                    OrderItems = o.OrderItems.Select(oi => new OrderItemSummaryDto
                    {
                        ProductId = oi.ProductId,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice,
                        TotalPrice = oi.TotalPrice,
                        Product = oi.Product != null ? new OrderProductDto
                        {
                            Id = oi.Product.Id,
                            ArName = oi.Product.ArName,
                            EnName = oi.Product.EnName,
                            ArDescription = oi.Product.ArDescription,
                            EnDescription = oi.Product.EnDescription,
                            Images = oi.Product.Images.Select(img => new OrderProductImageDto
                            {
                                Id = img.Id,
                                ImageUrl = img.ImageUrl,
                                CloudinaryPublicId = img.CloudinaryPublicId
                            }).ToList()
                        } : null
                    }).ToList(),
                    Payments = o.Payments != null ? o.Payments.Select(p => new PaymentSummaryDto
                    {
                        Id = p.Id,
                        Amount = p.Amount,
                        Currency = p.Currency,
                        Method = p.Method.ToString(),
                        Status = p.Status.ToString(),
                        TransactionId = p.TransactionId,
                        CreatedAt = p.CreatedAt,
                        PaymentLink = p.PaymentLink,
                        PaymentIntentionId = p.PaymentIntentionId,
                        PaymentLinkExpiresAt = p.PaymentLinkExpiresAt
                    }).ToList() : new List<PaymentSummaryDto>()
                })
        {
        }
    }
}
