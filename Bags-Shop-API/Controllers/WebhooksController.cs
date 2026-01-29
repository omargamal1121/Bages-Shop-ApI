using ApplicationLayer.Services.PaymentWebhookService;
using Bags_Shop_API.Services.PaymentServices;
using Bags_Shop_API.Services.PaymentServices.Dtos;
using Bags_Shop_API.Services.PaymentServices.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Bags_Shop_API.Controllers
{
    [Route("api/webhooks")]
    [ApiController]
    public class WebhooksController : ControllerBase
    {
        private readonly IPaymentWebhookService _webhookService;
        private readonly ILogger<WebhooksController> _logger;
        private readonly IMediator _mediator;

        public WebhooksController(
            IPaymentWebhookService webhookService, 
            ILogger<WebhooksController> logger,
            IMediator mediator)
        {
            _webhookService = webhookService;
            _logger = logger;
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

        [HttpPost("paymob")]
        public async Task<IActionResult> PaymobWebhook([FromQuery] string? hmac, [FromBody] PaymobWebhookDto dto)
        {
            
            
            if (string.IsNullOrEmpty(hmac))
            {
               
                _logger.LogWarning("HMAC missing from Paymob webhook request query.");
            }

            var success = await _webhookService.HandlePaymobAsync(dto, hmac ?? string.Empty);
            
            if (!success)
            {
                
                return BadRequest();
            }

            return Ok();
        }

    }
}
