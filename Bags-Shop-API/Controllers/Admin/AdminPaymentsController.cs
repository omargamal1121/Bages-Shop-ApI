using Bags_Shop_API.Services.PaymentServices.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Bags_Shop_API.Controllers.Admin
{
    [Route("api/admin/payments")]
    [ApiController]
    public class AdminPaymentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminPaymentsController(IMediator mediator)
        {
            _mediator = mediator;
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
