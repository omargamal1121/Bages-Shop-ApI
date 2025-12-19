using Bags_Shop_API.Services.PaymentServices;
using Bags_Shop_API.Services.PaymentServices.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Bags_Shop_API.Controllers
{
    [Route("api/webhooks")]
    [ApiController]
    public class WebhooksController : ControllerBase
    {
        private readonly IPaymentWebhookService _webhookService;
        private readonly ILogger<WebhooksController> _logger;

        public WebhooksController(IPaymentWebhookService webhookService, ILogger<WebhooksController> logger)
        {
            _webhookService = webhookService;
            _logger = logger;
        }

        [HttpPost("paymob")]
        public async Task<IActionResult> PaymobWebhook([FromBody] PaymobWebhookDto dto)
        {
            // Paymob sends HMAC as a query parameter 'hmac'
            string? receivedHmac = Request.Query["hmac"];
            
            if (string.IsNullOrEmpty(receivedHmac))
            {
                // Some Paymob versions might send it in a specific header or as part of the body
                // But usually it's a query param for the callback and webhook.
                _logger.LogWarning("HMAC missing from Paymob webhook request query.");
            }

            var success = await _webhookService.HandlePaymobAsync(dto, receivedHmac ?? string.Empty);
            
            if (!success)
            {
                // Paymob expects a 200 even if validation fails usually to stop retries if it's a "bad" webhook
                // but 400 is safer for debugging if we want them to retry on transient errors.
                // However, for success handling we MUST return 200.
                return BadRequest();
            }

            return Ok();
        }

        // Optional: Paymob callback (GET)
        [HttpGet("paymob")]
        public async Task<IActionResult> PaymobCallback([FromQuery] PaymobWebhookDto dto)
        {
            _logger.LogInformation("Paymob GET callback received: {@Dto}", dto);
             // Usually GET callbacks are for UI redirection, but we can log them.
            return Ok("Callback received");
        }
    }
}
