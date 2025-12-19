using Bags_Shop_API.Services.DiscountServices.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Bags_Shop_API.Controllers.User
{
    [Route("api/user/discounts")]
    [ApiController]
    public class UserDiscountsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserDiscountsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: api/user/discounts - Only active discounts
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GetAllDiscountsQuery query)
        {
            // Force filter to only active discounts
            query.IsActive = true;
            
            var result = await _mediator.Send(query);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        // GET: api/user/discounts/5 - Only if active
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var query = new GetDiscountByIdQuery(id);
            var result = await _mediator.Send(query);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            // Check if discount is active
            if (result.Data != null && !result.Data.IsActive)
                return NotFound(new { Success = false, Message = "Discount not found or not available" });

            return Ok(result);
        }
    }
}
