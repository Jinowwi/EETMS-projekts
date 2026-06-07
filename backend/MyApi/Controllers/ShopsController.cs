using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApi.Data;
using MyApi.Models;
using MyApi.Dto;
using MyApi.Services;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using BCrypt.Net;

namespace MyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShopsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly EmailService _emailService;
        private const int BcryptWorkFactor = 12;

        public ShopsController(AppDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService; 
        }

        // visu veikalu iegūšana DTO formātā
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShopDto>>> GetShops()
        {
            return await _context.Shops
                .Select(s => new ShopDto
                {
                    ShopID = s.ShopID,
                    Code = s.Code,
                    Country = s.Country.ToString(),
                    Address = s.Address,
                    Type = s.Type.ToString(),
                    Email = s.Email,
                    Password = null
                })
                .ToListAsync();
        }

        // veikala iegūšana pēc ID
        [HttpGet("{id}")]
        public async Task<ActionResult<ShopDto>> GetShop(int id)
        {
            var shop = await _context.Shops.FindAsync(id);
            if (shop == null)
                return NotFound();

            return new ShopDto
            {
                ShopID = shop.ShopID,
                Code = shop.Code,
                Country = shop.Country.ToString(),
                Address = shop.Address,
                Type = shop.Type.ToString(),
                Email = shop.Email,
                Password = null
            };
        }

        // jauna veikala pievienošana un paroles iestatīšanas e-pasta nosūtīšana
        [HttpPost]
        public async Task<ActionResult<ShopDto>> CreateShop([FromBody] ShopDto dto)
        {
            if (dto == null)
                return BadRequest("Request body is required");

            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest("Email is required");

            if (string.IsNullOrWhiteSpace(dto.Address))
                return BadRequest("Address is required");

            if (string.IsNullOrWhiteSpace(dto.Type))
                return BadRequest("Shop type is required");

            if (string.IsNullOrWhiteSpace(dto.Country))
                return BadRequest("Country is required");

            // veikala tipa pārveidošana uz enum
            if (!Enum.TryParse<ShopType>(dto.Type, ignoreCase: true, out var shopType))
                return BadRequest("Invalid shop type value");

            // valsts pārveidošana uz enum
            Country country;
            if (!Enum.TryParse<Country>(dto.Country, ignoreCase: true, out country))
            {
                if (int.TryParse(dto.Country, out var countryInt) && Enum.IsDefined(typeof(Country), countryInt))
                    country = (Country)countryInt;
                else
                    return BadRequest("Invalid country value");
            }

            var normalizedEmail = dto.Email.Trim().ToLower();

            // pārbaude, vai veikals ar šādu e-pastu jau eksistē
            var emailExists = await _context.Shops
                .AnyAsync(s => s.Email != null && s.Email.ToLower() == normalizedEmail);

            if (emailExists)
                return BadRequest("A shop with this email already exists");

            var shop = new Shop
            {
                Address = dto.Address.Trim(),
                Email = normalizedEmail,
                Country = country,
                Type = shopType,
                PasswordHash = string.Empty
            };

            _context.Shops.Add(shop);
            await _context.SaveChangesAsync();

            // veikala koda ģenerēšana pēc valsts un ID
            shop.Code = GenerateShopCode(shop.Country, shop.ShopID);
            await _context.SaveChangesAsync();

            // paroles iestatīšanas e-pasta nosūtīšana
            try
            {
                await _emailService.SendShopPasswordSetupEmailAsync(shop.Email!, shop.Code!);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Shop created, but email sending failed",
                    error = ex.Message,
                    shopId = shop.ShopID,
                    code = shop.Code
                });
            }

            return CreatedAtAction(nameof(GetShop), new { id = shop.ShopID }, new ShopDto
            {
                ShopID = shop.ShopID,
                Code = shop.Code,
                Country = shop.Country.ToString(),
                Address = shop.Address,
                Type = shop.Type.ToString(),
                Email = shop.Email,
                Password = null
            });
        }

        // veikala koda ģenerēšana pēc valsts prefiksa
        private static string GenerateShopCode(Country country, int shopId)
        {
            var prefix = country switch
            {
                Country.Lithuania => "LT",
                Country.Latvia => "LV",
                Country.Estonia => "EE",
                Country.Baltics => "BA",
                _ => "SH"
            };

            return $"{prefix}{shopId:D4}";
        }

        // veikala rediģēšana pēc ID
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateShop(int id, [FromBody] ShopDto dto)
        {
            if (dto == null)
                return BadRequest("Request body is required");

            if (id != dto.ShopID)
                return BadRequest("ID mismatch");

            if (!Enum.TryParse<ShopType>(dto.Type, out var shopType))
                return BadRequest("Invalid shop type value");

            if (!Enum.TryParse<Country>(dto.Country, out var country))
                return BadRequest("Invalid country value");

            var shop = await _context.Shops.FindAsync(id);
            if (shop == null)
                return NotFound();

            var normalizedEmail = dto.Email?.Trim().ToLower();
            if (!string.IsNullOrWhiteSpace(normalizedEmail))
            {
                // pārbaude, vai cits veikals jau neizmanto šo e-pastu
                var duplicateEmailExists = await _context.Shops
                    .AnyAsync(s => s.ShopID != id && s.Email != null && s.Email.ToLower() == normalizedEmail);
                
                if (duplicateEmailExists)
                    return BadRequest("Another shop already uses this email");
                
                shop.Email = normalizedEmail;
            }

            shop.Code = dto.Code?.Trim();
            shop.Address = dto.Address?.Trim();
            shop.Country = country;
            shop.Type = shopType;

            // paroles atjaunošana, ja tā ir norādīta
            if (!string.IsNullOrWhiteSpace(dto.Password))
            {
                shop.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, BcryptWorkFactor);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ShopExists(id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // veikala pieslēgšanās ar e-pastu un paroli
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDto dto)
        {
            if (dto == null)
                return BadRequest("Request body is required");

            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Email and password are required");

            var normalizedEmail = dto.Email.Trim().ToLower();
            var shop = await _context.Shops
                .FirstOrDefaultAsync(s => s.Email != null && s.Email.ToLower() == normalizedEmail);

            if (shop == null || string.IsNullOrWhiteSpace(shop.PasswordHash))
                return Unauthorized("Invalid email or password");

            // paroles pārbaude ar BCrypt
            var isValidPassword = BCrypt.Net.BCrypt.Verify(dto.Password, shop.PasswordHash);
            if (!isValidPassword)
                return Unauthorized("Invalid email or password");

            // paroles pārhashēšana, ja esošais hash ir novecojis
            if (BCrypt.Net.BCrypt.PasswordNeedsRehash(shop.PasswordHash, BcryptWorkFactor))
            {
                shop.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, BcryptWorkFactor);
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                shopId = shop.ShopID,
                code = shop.Code,
                email = shop.Email
            });
        }

        // veikala dzēšana pēc ID
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShop(int id)
        {
            var shop = await _context.Shops.FindAsync(id);
            if (shop == null)
                return NotFound();

            _context.Shops.Remove(shop);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // paroles iestatīšana veikalam pēc e-pasta
        [HttpPost("set-password")]
        public async Task<IActionResult> SetPassword([FromBody] SetPasswordByEmailDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email)) return BadRequest("Email is required");
            if (string.IsNullOrWhiteSpace(dto.Password)) return BadRequest("Password is required");

            var normalizedEmail = dto.Email.Trim().ToLower();
            var shop = await _context.Shops
                .FirstOrDefaultAsync(s => s.Email != null && s.Email.ToLower() == normalizedEmail);

            if (shop == null) return NotFound("Shop not found");

            shop.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, BcryptWorkFactor);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Password set successfully" });
        }

        // pārbaude, vai veikals eksistē pēc ID
        private bool ShopExists(int id)
        {
            return _context.Shops.Any(e => e.ShopID == id);
        }
    }
}