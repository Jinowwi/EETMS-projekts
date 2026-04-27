using Microsoft.AspNetCore.Mvc;

namespace MyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            return Ok(new
            {
                id = id,
                name = $"Item {id}",
                description = "Test item from C# API"
            });
        }

        [HttpPost]
        public IActionResult Post([FromBody] CreateItemRequest request)
        {
            return Ok(new
            {
                success = true,
                message = "Item created successfully",
                data = request
            });
        }
    }

    public class CreateItemRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty; 
    }
}