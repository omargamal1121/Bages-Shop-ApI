using Bags_Shop_API.Services.PaymentServices.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Bags_Shop_API.Controllers.Admin
{
    [Route("api/admin/webhooks")]
    [ApiController]
    public class AdminWebhooksController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminWebhooksController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GetAllWebhooksQuery query)
        {
            var result = await _mediator.Send(query);
            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _mediator.Send(new GetWebhookByIdQuery(id));
            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }
    }
}
