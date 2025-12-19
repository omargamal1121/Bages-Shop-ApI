using Bags_Shop_API.Services.ImageServices.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Bags_Shop_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ImagesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // POST: api/Images/product/5
        [HttpPost("product/{productId}")]
        public async Task<IActionResult> UploadProductImage(int productId, [FromForm] AddImageDto image)
        {
            var command = new SaveImageCommand(image.image, productId, isProduct: true);
            var result = await _mediator.Send(command);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        // POST: api/Images/product/5/multiple
        [HttpPost("product/{productId}/multiple")]
        public async Task<IActionResult> UploadProductImages(int productId, [FromForm] AddImagesDto images)
        {
            var command = new SaveImagesCommand(images.images, productId, isProduct: true);
            var result = await _mediator.Send(command);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        // POST: api/Images/collection/5
        [HttpPost("collection/{collectionId}")]
        public async Task<IActionResult> UploadCollectionImage(int collectionId, [FromForm] AddImageDto image)
        {
            var command = new SaveImageCommand(image.image, collectionId, isProduct: false);
            var result = await _mediator.Send(command);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        // POST: api/Images/collection/5/multiple
        [HttpPost("collection/{collectionId}/multiple")]
        public async Task<IActionResult> UploadCollectionImages(int collectionId, [FromForm] AddImagesDto images)
        {
            var command = new SaveImagesCommand(images.images, collectionId, isProduct: false);
            var result = await _mediator.Send(command);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        // DELETE: api/Images/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var command = new DeleteImageCommand(id);
            var result = await _mediator.Send(command);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }
    }
    public class AddImagesDto
    {
       
     public   List<IFormFile> images { get; set;  }
    }
    public class AddImageDto
    {
       
     public   IFormFile image { get; set;  }
    }
}
