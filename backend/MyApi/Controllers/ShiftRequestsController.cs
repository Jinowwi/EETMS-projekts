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
        
        // pieprasījumu saraksta iegūšana kopā ar piesaistītu iemeslu, uzņēmumu, veikalu, adminu, plānoto maiņu un vērtējumu datiem
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var requests = await _context.ShiftRequests
                .Include(r => r.Reason)
                .Include(r => r.Company)
                .Include(r => r.Shop)
                .Include(r => r.Rem)
                .Include(r => r.PlannedShift)
                .Include(r => r.Rating)
                .ToListAsync();

            return Ok(requests);
        }

        // pieprasījuma apstiprināšana un admina (REM) piesaistīšana
        [HttpPatch("{id}/accept")]
        public async Task<IActionResult> AcceptShiftRequest(int id, [FromBody] AcceptShiftRequestDto dto)
        {
            var request = await _context.ShiftRequests.FindAsync(id);
            if (request == null)
                return NotFound();

            request.RemId = dto.RemId;
            request.Status = ShiftRequestStatus.Approved;

            await _context.SaveChangesAsync();

            return Ok(request);
        }

        // pieprasījumu iegūšana pēc veikalu ID
        [HttpGet("byshop/{shopId}")]
        public async Task<IActionResult> GetByShop(int shopId)
        {
            var requests = await _context.ShiftRequests
                .Include(r => r.Reason)
                .Include(r => r.Rem)
                .Include(r => r.Company)
                .Include(r => r.Shop)
                .Include(r => r.PlannedShift)
                .Include(r => r.Rating)
                .Where(r => r.ShopId == shopId)
                .ToListAsync();

            return Ok(requests);
        }

        // jaunā pieprasījuma izveidošana
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

        // pieprasījumu dzēšana
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShiftRequest(int id)
        {
            var request = await _context.ShiftRequests.FindAsync(id);
            if (request == null)
                return NotFound();

            _context.ShiftRequests.Remove(request);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // pieprasījuma statusa un uzņēmuma rediģēšana pēc ID
        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdateShiftRequest(int id, [FromBody] UpdateShiftRequestDto dto)
        {
            var request = await _context.ShiftRequests.FindAsync(id);
            if (request == null)
                return NotFound();

            request.Status = (ShiftRequestStatus)dto.Status;
            request.CompanyId = dto.CompanyId;

            await _context.SaveChangesAsync();

            return Ok(request);
        }

        // ieplānotās maiņas pievienošana pieprasījumam
        [HttpPatch("{id}/planned-shift")]
        public async Task<IActionResult> SavePlannedShift(int id, [FromBody] PlannedShiftDto dto)
        {
            var request = await _context.ShiftRequests
                .Include(r => r.PlannedShift)
                .FirstOrDefaultAsync(r => r.ShiftRequestID == id);

            // pārbaude, vai pieprasījums eksistē
            if (request == null)
                return NotFound();

            // pārbaude vai pieprasījumām eksistē plānotā maiņa
            if (request.PlannedShift == null)
            {
                request.PlannedShift = new PlannedShifts
                {
                    ShiftRequestID = request.ShiftRequestID
                };
                _context.PlannedShifts.Add(request.PlannedShift);
            }

            request.PlannedShift.approx_date = dto.ApproxDate;
            request.PlannedShift.approx_start_time = dto.ApproxStartTime;
            request.PlannedShift.approx_duration = dto.ApproxDuration;

            await _context.SaveChangesAsync();
            return Ok(request.PlannedShift);
        }
    }
}