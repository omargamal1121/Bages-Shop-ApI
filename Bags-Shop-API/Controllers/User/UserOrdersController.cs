using Bags_Shop_API.ContextFile;
using Bags_Shop_API.Models;
using Bags_Shop_API.Services.OrderServices.Commands;
using Bags_Shop_API.Services.OrderServices.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Bags_Shop_API.Controllers.User
{
    [Route("api/user/orders")]
    [ApiController]
    public class UserOrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserOrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderCommand command)
        {
            command.Userkey = HttpContext.Items["UserKey"]?.ToString() ?? string.Empty;
            var result = await _mediator.Send(command);
            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders(
            [FromQuery] DateTime? createdFrom = null,
            [FromQuery] DateTime? createdTo = null,
            [FromQuery] OrderStatus? status = null,
            [FromQuery] int page = 1, 
            [FromQuery] int pageSize = 10)
        {
            var userKey = HttpContext.Items["UserKey"]?.ToString();
            
            if (string.IsNullOrEmpty(userKey))
                return BadRequest(new { Success = false, Message = "User key not found in cookies." });

            var query = new GetOrdersByUserKeyQuery(userKey)
            {
                CreatedFrom = createdFrom,
                CreatedTo = createdTo,
                Status = status,
                Page = page,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);
            
            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var userKey = HttpContext.Items["UserKey"]?.ToString();
            
            if (string.IsNullOrEmpty(userKey))
                return BadRequest(new { Success = false, Message = "User key not found in cookies." });

            var query = new GetOrderByIdAndUserKeyQuery(id, userKey);
            var result = await _mediator.Send(query);
            
            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }
    }
}
