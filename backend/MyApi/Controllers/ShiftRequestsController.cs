using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApi.Data;
using MyApi.Models;

namespace MyApi.Controllers
{
    [ApiController]
    [Route("api/shiftrequests")]
    public class ShiftRequestsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ShiftRequestsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/shiftrequests/byshop/{shopId}
        [HttpGet("byshop/{shopId}")]
        public async Task<IActionResult> GetByShop(int shopId)
        {
            var requests = await _context.ShiftRequests
                .Include(r => r.Reason)
                .Include(r => r.Rem)
                .Include(r => r.Company)
                .Where(r => r.ShopId == shopId &&
                            r.Status != ShiftRequestStatus.Done)
                .ToListAsync();

            return Ok(requests);
        }

        // POST: api/shiftrequests
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ShiftRequestDto dto)
        {
            var request = new ShiftRequest
            {
                Description = dto.Description,
                Status = ShiftRequestStatus.Sent,
                ReasonID = dto.ReasonID,
                ShopId = dto.ShopId,
                CompanyId = dto.CompanyId
            };

            _context.ShiftRequests.Add(request);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetByShop),
                new { shopId = request.ShopId }, request);
        }
    }
}