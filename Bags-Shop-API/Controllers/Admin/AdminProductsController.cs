using Bags_Shop_API.Services.ImageServices.Commands;
using Bags_Shop_API.Services.ProductServices.Command;
using Bags_Shop_API.Services.ProductServices.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Bags_Shop_API.Controllers.Admin
{
    [Route("api/admin/products")]
    [ApiController]
    public class AdminProductsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: api/admin/products
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GetAllProductsQuery query)
        {
      
            query.IsAdminRequest = true;
            
            var result = await _mediator.Send(query);
            
            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

  
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var query = new GetProductByIdQuery(id,true);
            var result = await _mediator.Send(query);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        // POST: api/admin/products
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return CreatedAtAction(nameof(GetById), new { id = result.Data }, result);
        }

        // PUT: api/admin/products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateProductCommand command)
        {
            if (id != command.Id)
                return BadRequest("ID mismatch");

            var result = await _mediator.Send(command);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        // DELETE: api/admin/products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var command = new DeleteProductCommand(id);
            var result = await _mediator.Send(command);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        // POST: api/admin/products/5/activate
        [HttpPost("{id}/activate")]
        public async Task<IActionResult> Activate(int id)
        {
            var command = new ToggleProductActiveCommand(id, true);
            var result = await _mediator.Send(command);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        // POST: api/admin/products/5/deactivate
        [HttpPost("{id}/deactivate")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var command = new ToggleProductActiveCommand(id, false);
            var result = await _mediator.Send(command);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        // POST: api/admin/products/5/images
        [HttpPost("{id}/images")]
        public async Task<IActionResult> AddImages(int id, [FromForm] List<IFormFile> images)
        {
            var command = new SaveImagesCommand(images, id, isProduct: true);
            var result = await _mediator.Send(command);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        // DELETE: api/admin/products/5/images/10
        [HttpDelete("{productId}/images/{imageId}")]
        public async Task<IActionResult> DeleteImage(int productId, int imageId)
        {
            var command = new DeleteImageCommand(imageId);
            var result = await _mediator.Send(command);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }
    }
}
