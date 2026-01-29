using Bags_Shop_API.Services.PaymentServices.Commands;
using Bags_Shop_API.Services.PaymentServices.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Bags_Shop_API.Controllers.User
{
    [Route("api/user/payments")]
    [ApiController]
    public class UserPaymentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserPaymentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePaymentCommand command)
        {
            var result = await _mediator.Send(command);
            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _mediator.Send(new GetPaymentByIdQuery(id));
            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetByOrderId(int orderId)
        {
            var result = await _mediator.Send(new GetPaymentsByOrderIdQuery(orderId));
            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }
    }
}
