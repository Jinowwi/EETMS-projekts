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
    public class ShopsController : ControllerBase
    {
        private readonly AppDbContext _context; 

        public ShopsController(AppDbContext context)
        {
            _context = context; 
        }

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
                    Password = s.PasswordHash
                })
                .ToListAsync(); 
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Shop>> GetShop(int id)
        {
            var shop = await _context.Shops.FindAsync(id); 
            if (shop == null)
                return NotFound(); 

            return shop; 
        }

        [HttpPost]
        public async Task<ActionResult<Shop>> PostShop(Shop shop)
        {
            _context.Shops.Add(shop);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetShop", new { id = shop.ShopID }, shop);
        }
        
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateShop(int id, ShopDto dto)
        {
            if (id != dto.ShopID)
            {
                return BadRequest("ID mismatch"); 
            }

            var shop = await _context.Shops.FindAsync(id); 

            if (shop == null)
            {
                return NotFound(); 
            }

            if (!Enum.TryParse<ShopType>(dto.Type, out var shopType))
            {
                return BadRequest("Invalid shop type value"); 
            }

            if (!Enum.TryParse<Country>(dto.Country, out var country))
            {
                return BadRequest("Invalid country value");
            }

            shop.Type = shopType; 
            shop.Address = dto.Address; 
            shop.Country = country; 
            shop.Code = dto.Code; 

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ShopExists(id)) 
                {
                    return NotFound(); 
                }
                throw; 
            }
            return NoContent(); 
        }
        
        private bool ShopExists(int id)
        {
            return _context.Shops.Any(e => e.ShopID == id); 
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShop(int id)
        {
            var shop = await _context.Shops.FindAsync(id);
            if (shop == null)
            {
                return NotFound();
            }

            _context.Shops.Remove(shop);
            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
