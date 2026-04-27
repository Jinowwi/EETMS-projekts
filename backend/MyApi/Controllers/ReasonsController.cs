using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 
using MyApi.Data; 
using MyApi.Models; 
using MyApi.Dto;

namespace MyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReasonsController : ControllerBase
    {
        private readonly AppDbContext _context; 

        public ReasonsController (AppDbContext context)
        {
            _context = context; 
        }

        [HttpGet] 
        public async Task<ActionResult<IEnumerable<ReasonsDto>>> GetReason()
        {
            return await (from r in _context.Reasons
                select new ReasonsDto
                {
                    ReasonID = r.ReasonID, 
                    Name = r.Name, 
                }).ToListAsync(); 
        }

        [HttpGet("bycompany/{companyId}")]
        public async Task<ActionResult<IEnumerable<ReasonsDto>>> GetReasonsByCompany(int companyId)
        {
            return await (
                from cr in _context.CompanyReasons
                join r in _context.Reasons on cr.ReasonsReasonID equals r.ReasonID
                where cr.CompaniesCompanyID == companyId
                select new ReasonsDto
                {
                    ReasonID = r.ReasonID,
                    Name = r.Name,
                }
            ).ToListAsync();
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<Reason>> GetReason(int id)
        {
            var reason = await _context.Reasons.FindAsync(id); 
            if (reason == null)
                return NotFound(); 
            
            return reason; 
        }

        [HttpPost]
        public async Task<ActionResult<Reason>> CreateReason(Reason reason)
        {
            _context.Reasons.Add(reason); 
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetReason), new { id = reason.ReasonID }, reason); 
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReason(int id, Reason reason)
        {
            if (id != reason.ReasonID)
                return BadRequest(); 
            _context.Entry(reason).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            } 
            catch (DbUpdateConcurrencyException)
            {
                if (!ReasonExists(id)) 
                    return NotFound();
                else 
                    throw; 
            }

            return NoContent(); 
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReason(int id)
        {
            var reason = await _context.Reasons.FindAsync(id); 
            if (reason == null) 
                return NotFound(); 

            var linkedCompanyReasons = _context.CompanyReasons.Where(cr => 
        cr.ReasonsReasonID == id);

            _context.CompanyReasons.RemoveRange(linkedCompanyReasons);
        await _context.SaveChangesAsync();

            _context.Reasons.Remove(reason);
        
            await _context.SaveChangesAsync(); 

            return NoContent();
        }

        private bool ReasonExists(int id)
        {
            return _context.Reasons.Any(r => r.ReasonID == id); 
        }
    }
}
