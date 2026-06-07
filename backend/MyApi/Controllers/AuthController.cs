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

        // kontroliera inicializācija ar datu bāzes kontekstu
        public AuthController(AppDbContext context)
        {
            _context = context;
        }

        // lietotāja pieteikšanās apstrāde
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            // Meklēšana pēc e-pasta
            var admin = await _context.Administrators
                .FirstOrDefaultAsync(a => a.Email == dto.Email);

            // paroles pārbaude, kļūdas paziņojums
            if (admin == null || !admin.VerifyPassword(dto.Password))
                return Unauthorized(new { message = "Invalid email or password" });

            // lietotāja datu atgriešana
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