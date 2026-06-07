using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApi.Data;
using MyApi.Models;
using MyApi.Dto;
using MyApi.Services;
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
        private readonly EmailService _emailService;
        private const int BcryptWorkFactor = 12;

        // kontroliera inicializācija ar datu bāzes kontekstu un e-pasta servisu
        public CompaniesController(AppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // uzņēmumu iegūšana un grupēšana pēc administrātora
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CompanyDto>>> GetCompanies()
        {
            var companies = await _context.Companies.ToListAsync();
            var admins = await _context.Administrators.ToListAsync();

            var result = companies.Select(c =>
            {
                // administrātoru atrāšana pēc RemID lauka pie uzņēmumiem
                var admin = admins.FirstOrDefault(a => a.RemID == c.RemID);

                // uzņēmumu datus pārveidošana par DTO ar admina e-pastu
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
                    RimiEmployeeEmail = admin?.Email,
                    Password = null
                };
            }).ToList();

            return Ok(result);
        }

        // viena uzņēmuma iegūšana pēc ID
        [HttpGet("{id}")]
        public async Task<ActionResult<CompanyDto>> GetCompany(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null)
                return NotFound();

            // piesaistītā admina ielādē, ja RemID ir norādīts
            var admin = company.RemID.HasValue
                ? await _context.Administrators.FirstOrDefaultAsync(a => a.RemID == company.RemID.Value)
                : null;

            // uzņēmumu datu pārveidošana par DTO ar admina e-pastu
            var result = new CompanyDto
            {
                CompanyID = company.CompanyID,
                CompanyName = company.CompanyName ?? "",
                Address = company.Address ?? "",
                Country = (int)company.Country,
                RemID = company.RemID ?? 0,
                PhoneNumber = company.PhoneNumber,
                RegistrationNumber = company.RegistrationNumber,
                Email = company.Email,
                RimiEmployeeEmail = admin?.Email,
                Password = null
            };

            return Ok(result);
        }

        // jaunā uzņēmuma veidošana
        [HttpPost]
        public async Task<ActionResult<CompanyDto>> CreateCompany([FromBody] CompanyDto dto)
        {
            // pārbaude, vai oligātie lauki ir aizpildīti
            if (dto == null)
                return BadRequest("Request body is required");

            if (string.IsNullOrWhiteSpace(dto.CompanyName))
                return BadRequest("Company name is required");

            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest("Email is required");

            if (!Enum.IsDefined(typeof(Country), dto.Country))
                return BadRequest("Invalid country value");

            // e-pasta normalizēšana un unikalitātes pārbaude
            var normalizedEmail = dto.Email.Trim().ToLower();

            var emailExists = await _context.Companies
                .AnyAsync(c => c.Email != null && c.Email.ToLower() == normalizedEmail);

            if (emailExists)
                return BadRequest("A company with this email already exists");

            // jaunā uzņēmuma objekta izveidošanā un datu pievienošana datubāzē
            var company = new Company
            {
                CompanyName = dto.CompanyName.Trim(),
                Address = dto.Address?.Trim(),
                PhoneNumber = dto.PhoneNumber?.Trim(),
                RegistrationNumber = dto.RegistrationNumber?.Trim(),
                Email = normalizedEmail,
                Country = (Country)dto.Country,
                RemID = dto.RemID == 0 ? null : dto.RemID,
                PasswordHash = string.Empty
            };

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            // e-pasta nosūtīšana paroles iestatīšanai
            try
            {
                await _emailService.SendCompanyPasswordSetupEmailAsync(
                    company.Email!,
                    company.CompanyName ?? "Company" 
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Company created, but email sending failed: {ex.Message}");
            }

            // uzņēmumu datu pārveidošana par DTO
            var result = new CompanyDto
            {
                CompanyID = company.CompanyID,
                CompanyName = company.CompanyName,
                Address = company.Address,
                Country = (int)company.Country,
                RemID = company.RemID ?? 0,
                PhoneNumber = company.PhoneNumber,
                RegistrationNumber = company.RegistrationNumber,
                Email = company.Email,
                Password = null
            };

            return CreatedAtAction(nameof(GetCompany), new { id = company.CompanyID }, result);
        }

        // uzņēmuma paroles iestatīšana pēc e-pasta
        [HttpPost("set-password")]
        public async Task<IActionResult> SetPassword([FromBody] SetPasswordByEmailDto dto)
        {
            // pārbaude, vai viss nepieciešamais ir aizpildīts
            if (dto == null)
                return BadRequest("Request body is required");

            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest("Email is required");

            if (string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Password is required");

            // e-pasta normalizēšana un uzņēmuma meklēšana pēc e-pasta
            var normalizedEmail = dto.Email.Trim().ToLower();

            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Email != null && c.Email.ToLower() == normalizedEmail);

            if (company == null)
                return NotFound("Company not found");

            // paroles saglabāšana
            company.SetPassword(dto.Password);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password set successfully" });
        }

        // uzņēmumu datu atjaunošana (rediģēšana)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCompany(int id, [FromBody] CompanyDto dto)
        {
            // pārbaude, vai viss nepieciešamais ir pareizi aizpildīts
            if (dto == null)
                return BadRequest("Request body is required");

            if (id != dto.CompanyID)
                return BadRequest("ID mismatch");

            if (string.IsNullOrWhiteSpace(dto.CompanyName))
                return BadRequest("Company name is required");

            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest("Email is required");

            if (!Enum.IsDefined(typeof(Country), dto.Country))
                return BadRequest("Invalid country value");

            var company = await _context.Companies.FindAsync(id);
            if (company == null)
                return NotFound();

            var normalizedEmail = dto.Email.Trim().ToLower();

            var duplicateEmailExists = await _context.Companies
                .AnyAsync(c => c.CompanyID != id && c.Email != null && c.Email.ToLower() == normalizedEmail);

            if (duplicateEmailExists)
                return BadRequest("Another company already uses this email");

            // lauku aizpildīšana
            company.CompanyName = dto.CompanyName.Trim();
            company.Address = dto.Address?.Trim();
            company.PhoneNumber = dto.PhoneNumber?.Trim();
            company.RegistrationNumber = dto.RegistrationNumber?.Trim();
            company.Email = normalizedEmail;
            company.Country = (Country)dto.Country;
            company.RemID = dto.RemID == 0 ? null : dto.RemID;

            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                company.SetPassword(dto.Password);
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // uzņēmuma pērstāvja pieteikšanās apstrāde
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDto dto)
        {
            // pārbaude, vai viss nepieciešamais ir noradīts
            if (dto == null)
                return BadRequest("Request body is required");

            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Email and password are required");

            var normalizedEmail = dto.Email.Trim().ToLower();

            // Meklēšana pēc e-pasta
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Email != null && c.Email.ToLower() == normalizedEmail);

            // paroles pārbaude, kļūdas paziņojums
            if (company == null || string.IsNullOrWhiteSpace(company.PasswordHash))
                return Unauthorized("Invalid email or password");

            if (!company.VerifyPassword(dto.Password))
                return Unauthorized("Invalid email or password");

            if (BCrypt.Net.BCrypt.PasswordNeedsRehash(company.PasswordHash, BcryptWorkFactor))
            {
                company.SetPassword(dto.Password);
                await _context.SaveChangesAsync();
            }

            // uzņēmuma datu atgriešana
            return Ok(new
            {
                companyId = company.CompanyID,
                companyName = company.CompanyName,
                email = company.Email
            });
        }

        // uzņēmuma dzēšana pēc ID
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null)
                return NotFound();

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }

    // DTO paroles iestatīšanai pēc e-pasta
    public class SetPasswordByEmailDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}