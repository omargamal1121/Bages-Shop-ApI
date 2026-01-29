using Bags_Shop_API.Services.OrderServices.Commands;
using Bags_Shop_API.Services.OrderServices.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Bags_Shop_API.Controllers.Admin
{
    [Route("api/admin/orders")]
    [ApiController]
    public class AdminOrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminOrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GetAllOrdersQuery query)
        {
            var result = await _mediator.Send(query);
            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _mediator.Send(new GetOrderByIdQuery(id));
            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusCommand command)
        {
            // Ensure the ID from the route matches the command
            command.OrderId = id;

            var result = await _mediator.Send(command);
            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }
    }
}
