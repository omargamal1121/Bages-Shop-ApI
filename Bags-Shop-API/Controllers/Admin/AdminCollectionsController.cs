using Bags_Shop_API.Services.CollectionServices.Commands;
using Bags_Shop_API.Services.CollectionServices.Queries;
using Bags_Shop_API.Services.ImageServices.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Bags_Shop_API.Controllers.Admin
{
    [Route("api/admin/collections")]
    [ApiController]
    public class AdminCollectionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminCollectionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: api/admin/collections
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GetAllCollectionsQuery query)
        {
            query.IsAdminRequest = true;
            var result = await _mediator.Send(query);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        // GET: api/admin/collections/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var query = new GetCollectionByIdQuery(id);
            var result = await _mediator.Send(query);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        // POST: api/admin/collections
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCollectionCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return CreatedAtAction(nameof(GetById), new { id = result.Data }, result);
        }

        // PUT: api/admin/collections/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateCollectionCommand command)
        {
            if (id != command.Id)
                return BadRequest("ID mismatch");

            var result = await _mediator.Send(command);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        // DELETE: api/admin/collections/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var command = new DeleteCollectionCommand(id);
            var result = await _mediator.Send(command);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        // POST: api/admin/collections/5/activate
        [HttpPost("{id}/activate")]
        public async Task<IActionResult> Activate(int id)
        {
            var command = new ToggleCollectionActiveCommand(id, true);
            var result = await _mediator.Send(command);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        // POST: api/admin/collections/5/deactivate
        [HttpPost("{id}/deactivate")]
        public async Task<IActionResult> Deactivate(int id)
        {
            var command = new ToggleCollectionActiveCommand(id, false);
            var result = await _mediator.Send(command);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        // POST: api/admin/collections/5/images
        [HttpPost("{id}/images")]
        public async Task<IActionResult> AddImages(int id, [FromForm] List<IFormFile> images)
        {
            var command = new SaveImagesCommand(images, id, isProduct: false);
            var result = await _mediator.Send(command);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        // POST: api/admin/collections/{collectionId}/products
        [HttpPost("{collectionId}/products")]
        public async Task<IActionResult> AddProductsToCollection(int collectionId, [FromBody] List<int> productIds)
        {
            var command = new AddProductToCollectionCommand(collectionId, productIds);
            var result = await _mediator.Send(command);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        // DELETE: api/admin/collections/products
        [HttpDelete("products")]
        public async Task<IActionResult> RemoveProductsFromCollection([FromBody] List<int> productIds)
        {
            var command = new RemoveProductFromCollectionCommand(productIds);
            var result = await _mediator.Send(command);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        // DELETE: api/admin/collections/5/images/10
        [HttpDelete("{collectionId}/images/{imageId}")]
        public async Task<IActionResult> DeleteImage(int collectionId, int imageId)
        {
            var command = new DeleteImageCommand(imageId);
            var result = await _mediator.Send(command);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }
    }
}
