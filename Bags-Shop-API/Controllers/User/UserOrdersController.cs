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
            var result = await _mediator.Send(command);
            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }
    }
}
