using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; 
using MyApi.Data; 
using MyApi.Models; 
using MyApi.Dto;

[Route("api/[controller]")]
[ApiController]
public class AdministratorsController : ControllerBase
{
    private readonly AppDbContext _context; 

    // kontroliera inicializācija ar datu bāzes kontekstu
    public AdministratorsController(AppDbContext context)
    {
        _context = context; 
    }
    
    // administratoru saraksta iegūšana
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Administration>>> GetAdministrators()
    {
        return await _context.Administrators.ToListAsync();         
    }
}