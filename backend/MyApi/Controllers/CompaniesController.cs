using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApi.Data;
using MyApi.Models;
using MyApi.Dto;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace MyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompaniesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CompaniesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
public async Task<ActionResult<IEnumerable<CompanyDto>>> GetCompanies()
{
    var companies = await _context.Companies.ToListAsync();
    var admins = await _context.Administrators.ToListAsync();

    return Ok(companies.Select(c => {
        var admin = admins.FirstOrDefault(a => a.RemID == c.RemID);
        return new CompanyDto
        {
            CompanyID = c.CompanyID,
            CompanyName = c.CompanyName ?? "",
            Address = c.Address ?? "",
            Country = (int)c.Country,
            RemID = c.RemID ?? 0,
            PhoneNumber = c.PhoneNumber,
            RegistrationNumber = c.RegistrationNumber,
            Email = c.Email,
            RimiEmployeeEmail = admin?.Email,  // ✅ from Administration table
            Password = null
        };
    }).ToList());
}

    [HttpGet("{id}")]
public async Task<ActionResult<CompanyDto>> GetCompany(int id)
{
    var company = await _context.Companies.FindAsync(id);
    if (company == null) return NotFound();

    var admin = company.RemID.HasValue
        ? await _context.Administrators.FindAsync(company.RemID.Value)
        : null;

    return Ok(new CompanyDto
    {
        CompanyID = company.CompanyID,
        CompanyName = company.CompanyName ?? "",
        Address = company.Address ?? "",
        Country = (int)company.Country,
        RemID = company.RemID ?? 0,
        PhoneNumber = company.PhoneNumber,
        RegistrationNumber = company.RegistrationNumber,
        Email = company.Email,
        RimiEmployeeEmail = admin?.Email,  // ✅ from Administration table
        Password = null
    });
}



        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCompany(int id, CompanyDto dto)
        {
            if (id != dto.CompanyID)
            {
                return BadRequest("ID mismatch");
            }

            var company = await _context.Companies.FindAsync(id);
            
            if (company == null)
            {
                return NotFound();
            }

            if (!Enum.IsDefined(typeof(Country), dto.Country))
            {
                return BadRequest("Invalid country value");
            }

            company.CompanyName = dto.CompanyName;
            company.Address = dto.Address;
            company.Country = (Country)dto.Country;
            company.RemID = dto.RemID;
            company.PhoneNumber = dto.PhoneNumber;
            company.RegistrationNumber = dto.RegistrationNumber;
            company.Email = dto.Email;

            if (!string.IsNullOrWhiteSpace(dto.Password))
                company.PasswordHash = dto.Password; 
            
            try
            {
                var entry = _context.Entry(company);
foreach (var prop in entry.Properties)
{
    Console.WriteLine($"{prop.Metadata.Name}: Original={prop.OriginalValue}, Current={prop.CurrentValue}, Modified={prop.IsModified}");
}
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CompanyExists(id))
                {
                    if (!CompanyExists(id)) return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null)
            {
                return NotFound();
            }

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost]
public async Task<ActionResult<CompanyDto>> CreateCompany(CompanyDto dto)
{
    var company = new Company
    {
        CompanyName = dto.CompanyName,
        Address = dto.Address,
        PhoneNumber = dto.PhoneNumber,
        RegistrationNumber = dto.RegistrationNumber,
        Email = dto.Email,
        PasswordHash = dto.Password,
        Country = (Country)(dto.Country == 0 ? 2 : dto.Country), 
        RemID = dto.RemID == 0 ? null : dto.RemID
    };

    _context.Companies.Add(company);
    await _context.SaveChangesAsync();

    return CreatedAtAction(nameof(GetCompany), new { id = company.CompanyID }, new CompanyDto
    {
        CompanyID = company.CompanyID,
        CompanyName = company.CompanyName,
        Email = company.Email,
        Country = (int)company.Country
    });
}

[HttpPost("login")]
public async Task<ActionResult> Login([FromBody] LoginDto dto)
{
    var company = await _context.Companies
        .FirstOrDefaultAsync(c => c.Email == dto.Email);

    if (company == null || !company.VerifyPassword(dto.Password))
        return Unauthorized("Invalid email or password");

    return Ok(new
    {
        companyId = company.CompanyID,
        companyName = company.CompanyName
    });
}

        private bool CompanyExists(int id)
        {
            return _context.Companies.Any(e => e.CompanyID == id);
        }
    }
}
