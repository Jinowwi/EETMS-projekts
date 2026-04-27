using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApi.Data;
using MyApi.Dto;
using Twilio.Types;

namespace MyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var admin = await _context.Administrators
                .FirstOrDefaultAsync(a => a.Email == dto.Email);

            if (admin == null || !admin.VerifyPassword(dto.Password))
                return Unauthorized(new { message = "Invalid email or password" });

            return Ok(new
            {
                remId = admin.RemID,
                email = admin.Email,
                firstName = admin.FirstName,
                lastName = admin.LastName,
                phoneNumber = admin.PhoneNumber,
                role = admin.TypeOfAdmin.ToString()
            });
        }
    }
}