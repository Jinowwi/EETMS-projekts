using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApi.Data;
using MyApi.Models;

namespace MyApi.Controllers
{
    [ApiController]
    [Route("api/ratings")]
    public class RatingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RatingsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var ratings = await _context.Ratings
                .Include(r => r.ShiftRequest)
                .ToListAsync();

            return Ok(ratings);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var rating = await _context.Ratings
                .Include(r => r.ShiftRequest)
                .FirstOrDefaultAsync(r => r.RatingID == id);

            if (rating == null)
                return NotFound();

            return Ok(rating);
        }

        [HttpGet("byshiftrequest/{shiftRequestId}")]
        public async Task<IActionResult> GetByShiftRequest(int shiftRequestId)
        {
            var rating = await _context.Ratings
                .Include(r => r.ShiftRequest)
                .FirstOrDefaultAsync(r => r.ShiftRequestID == shiftRequestId);

            if (rating == null)
                return NotFound();

            return Ok(rating);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] RatingDto dto)
        {
            if (dto.ShiftRequestID == null)
                return BadRequest("ShiftRequestID is required.");

            var shiftRequest = await _context.ShiftRequests
                .Include(sr => sr.Rating)
                .FirstOrDefaultAsync(sr => sr.ShiftRequestID == dto.ShiftRequestID.Value);

            if (shiftRequest == null)
                return NotFound("Shift request not found.");

            if (shiftRequest.Status != ShiftRequestStatus.Done)
                return BadRequest("Only done shift requests can be rated.");

            if (shiftRequest.Rating != null)
                return BadRequest("This shift request already has a rating.");

            var rating = new Ratings
            {
                Stars = dto.Stars,
                Comment = dto.Comment,
                ShiftRequestID = dto.ShiftRequestID.Value
            };

            _context.Ratings.Add(rating);
            await _context.SaveChangesAsync();

            return Ok(rating);
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> Patch(int id, [FromBody] RatingDto dto)
        {
            var rating = await _context.Ratings.FindAsync(id);

            if (rating == null)
                return NotFound();

            rating.Stars = dto.Stars;
            rating.Comment = dto.Comment;

            await _context.SaveChangesAsync();

            return Ok(rating);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] RatingDto dto)
        {
            var rating = await _context.Ratings.FindAsync(id);

            if (rating == null)
                return NotFound();

            rating.Stars = dto.Stars;
            rating.Comment = dto.Comment;

            await _context.SaveChangesAsync();

            return Ok(rating);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var rating = await _context.Ratings.FindAsync(id);

            if (rating == null)
                return NotFound();

            _context.Ratings.Remove(rating);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}