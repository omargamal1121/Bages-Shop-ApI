using Bags_Shop_API.Services.ProductServices.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Bags_Shop_API.Controllers.User
{
    [Route("api/user/products")]
    [ApiController]
    public class UserProductsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: api/user/products - Only active products
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GetAllProductsQuery query)
        {
            // Force filter to only active products
            query.IsActive = true;
            // IsAdminRequest defaults to false, so only active discounts will be returned
            
            var result = await _mediator.Send(query);
            
            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        // GET: api/user/products/5 - Only if active
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var query = new GetProductByIdQuery(id,true);
            var result = await _mediator.Send(query);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            // Check if product is active
            if (result.Data != null && !result.Data.IsActive)
                return NotFound(new { Success = false, Message = "Product not found or not available" });

            return Ok(result);
        }
    }
}
