using Bags_Shop_API.Services.DiscountServices.Commands;
using Bags_Shop_API.Services.DiscountServices.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Bags_Shop_API.Controllers.Admin
{
    [Route("api/admin/discounts")]
    [ApiController]
    public class AdminDiscountsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminDiscountsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: api/admin/discounts
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GetAllDiscountsQuery query)
        {
            query.IsAdminRequest = true;
            var result = await _mediator.Send(query);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        // GET: api/admin/discounts/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var query = new GetDiscountByIdQuery(id);
            query.IsAdminRequest = true;
            var result = await _mediator.Send(query);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        // POST: api/admin/discounts
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDiscountCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return CreatedAtAction(nameof(GetById), new { id = result.Data }, result);
        }

        // PUT: api/admin/discounts/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateDiscountCommand command)
        {
            if (id != command.Id)
                return BadRequest("ID mismatch");

            var result = await _mediator.Send(command);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        // DELETE: api/admin/discounts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var command = new DeleteDiscountCommand(id);
            var result = await _mediator.Send(command);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        // POST: api/admin/discounts/5/activate
        [HttpPost("{id}/activate")]
        public async Task<IActionResult> Activate(int id)
        {
            var command = new ToggleDiscountActiveCommand(id, true);
            var result = await _mediator.Send(command);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        // POST: api/admin/discounts/5/deactivate
        [HttpPost("{id}/deactivate")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var command = new ToggleDiscountActiveCommand(id, false);
            var result = await _mediator.Send(command);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }
        // POST: api/admin/discounts/{id}/products
        [HttpPost("{id}/products")]
        public async Task<IActionResult> AddProductsToDiscount(int id, [FromBody] List<int> productIds)
        {
            var command = new AddProductsToDiscountCommand(id, productIds);
            var result = await _mediator.Send(command);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        // DELETE: api/admin/discounts/products
        [HttpDelete("products")]
        public async Task<IActionResult> RemoveProductsFromDiscount([FromBody] List<int> productIds)
        {
            var command = new RemoveProductsFromDiscountCommand(productIds);
            var result = await _mediator.Send(command);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }
    }
}
