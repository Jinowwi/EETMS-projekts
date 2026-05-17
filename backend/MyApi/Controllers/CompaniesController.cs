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

        public CompaniesController(AppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CompanyDto>>> GetCompanies()
        {
            var companies = await _context.Companies.ToListAsync();
            var admins = await _context.Administrators.ToListAsync();

            var result = companies.Select(c =>
            {
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
                    RimiEmployeeEmail = admin?.Email,
                    Password = null
                };
            }).ToList();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CompanyDto>> GetCompany(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company == null)
                return NotFound();

            var admin = company.RemID.HasValue
                ? await _context.Administrators.FirstOrDefaultAsync(a => a.RemID == company.RemID.Value)
                : null;

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

        [HttpPost]
        public async Task<ActionResult<CompanyDto>> CreateCompany([FromBody] CompanyDto dto)
        {
            if (dto == null)
                return BadRequest("Request body is required");

            if (string.IsNullOrWhiteSpace(dto.CompanyName))
                return BadRequest("Company name is required");

            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest("Email is required");

            if (!Enum.IsDefined(typeof(Country), dto.Country))
                return BadRequest("Invalid country value");

            var normalizedEmail = dto.Email.Trim().ToLower();

            var emailExists = await _context.Companies
                .AnyAsync(c => c.Email != null && c.Email.ToLower() == normalizedEmail);

            if (emailExists)
                return BadRequest("A company with this email already exists");

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

        [HttpPost("set-password")]
        public async Task<IActionResult> SetPassword([FromBody] SetPasswordByEmailDto dto)
        {
            if (dto == null)
                return BadRequest("Request body is required");

            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest("Email is required");

            if (string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Password is required");

            var normalizedEmail = dto.Email.Trim().ToLower();

            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Email != null && c.Email.ToLower() == normalizedEmail);

            if (company == null)
                return NotFound("Company not found");

            company.SetPassword(dto.Password);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password set successfully" });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCompany(int id, [FromBody] CompanyDto dto)
        {
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

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDto dto)
        {
            if (dto == null)
                return BadRequest("Request body is required");

            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Email and password are required");

            var normalizedEmail = dto.Email.Trim().ToLower();

            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Email != null && c.Email.ToLower() == normalizedEmail);

            if (company == null || string.IsNullOrWhiteSpace(company.PasswordHash))
                return Unauthorized("Invalid email or password");

            if (!company.VerifyPassword(dto.Password))
                return Unauthorized("Invalid email or password");

            if (BCrypt.Net.BCrypt.PasswordNeedsRehash(company.PasswordHash, BcryptWorkFactor))
            {
                company.SetPassword(dto.Password);
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                companyId = company.CompanyID,
                companyName = company.CompanyName,
                email = company.Email
            });
        }

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

    public class SetPasswordByEmailDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}