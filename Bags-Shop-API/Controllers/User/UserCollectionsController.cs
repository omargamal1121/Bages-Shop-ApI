using Bags_Shop_API.Services.CollectionServices.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Bags_Shop_API.Controllers.User
{
    [Route("api/user/collections")]
    [ApiController]
    public class UserCollectionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UserCollectionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: api/user/collections - Only active collections
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GetAllCollectionsQuery query)
        {
            // Force filter to only active collections
            query.IsActive = true;
            
            var result = await _mediator.Send(query);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            return Ok(result);
        }

        // GET: api/user/collections/5 - Only if active
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var query = new GetCollectionByIdQuery(id);
            var result = await _mediator.Send(query);

            if (!result.Success)
                return StatusCode(result.StatusCode, result);

            // Check if collection is active
            if (result.Data != null && !result.Data.IsActive)
                return NotFound(new { Success = false, Message = "Collection not found or not available" });

            return Ok(result);
        }
    }
}
