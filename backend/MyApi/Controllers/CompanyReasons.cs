using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApi.Data;
using MyApi.Models;

namespace MyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompanyReasonsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CompanyReasonsController(AppDbContext context)
        {
            _context = context;
        }

        // visu uzņēmumu un iemeslu saišu iegūšana
        [HttpGet]
        public async Task<IActionResult> GetAllCompanyReasons()
        {
            var data = await _context.CompanyReasons.ToListAsync();
            return Ok(data);
        }

        // jaunu ierakstu veidošana (iemeslu piešķiršana uzņēmumam)
        [HttpPost]
        public async Task<IActionResult> AddCompanyReason([FromBody] CompanyReason newConnection)
        {
            if (newConnection == null) 
                return BadRequest();

            // pārbaude, vai šāda sasaite jau eksistē
            var exists = await _context.CompanyReasons.AnyAsync(cr => 
                cr.CompaniesCompanyID == newConnection.CompaniesCompanyID && 
                cr.ReasonsReasonID == newConnection.ReasonsReasonID);

            if (exists) 
                return Ok(newConnection);

            _context.CompanyReasons.Add(newConnection);
            await _context.SaveChangesAsync();
            
            return Ok(newConnection);
        }

        // ierakstu dzēšana
        [HttpDelete("{companyId}/{reasonId}")]
        public async Task<IActionResult> DeleteCompanyReason(int companyId, int reasonId)
        {
            var connection = await _context.CompanyReasons
                .FirstOrDefaultAsync(cr => cr.CompaniesCompanyID == companyId && cr.ReasonsReasonID == reasonId);

            if (connection == null) 
                return NotFound();

            // ja sasaistei ir maiņas, tad tās arī tiks dzēstas
            var attachedShifts = _context.Shifts.Where(s => s.CompanyReasonID == connection.CompanyReasonID);
            
            if (attachedShifts.Any()) 
            {
                _context.Shifts.RemoveRange(attachedShifts);
            }

            _context.CompanyReasons.Remove(connection);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
