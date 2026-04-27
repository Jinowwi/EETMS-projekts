using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyApi.Data;
using MyApi.Models;
using MyApi.Dto;
using Microsoft.AspNetCore.JsonPatch;

namespace MyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShiftsController : ControllerBase
    {
        private readonly AppDbContext _context; 

        public ShiftsController(AppDbContext context)
        {
            _context = context; 
        }

        [HttpGet] 
        public async Task<ActionResult<IEnumerable<ShiftsDto>>> GetShifts()
        {
            return await _context.Shifts
                .Include(s => s.CompanyReason)
                .ThenInclude(cr => cr.Company)
                .Include(s => s.CompanyReason)
                .ThenInclude(cr => cr.Reason)
                .Include(s => s.Shop)
                .Select(s => new ShiftsDto
                {
                    ShiftID = s.ShiftID,
                    startDate = s.start_date,
                    EndDate = s.end_date,
                    startTime = s.start_time,
                    endTime = s.end_time,
                    CompanyReasonID = s.CompanyReasonID,
                    CompanyName = s.CompanyReason != null ? s.CompanyReason.Company.CompanyName : null, 
                    ReasonName = s.CompanyReason != null ? s.CompanyReason.Reason.Name : null,
                    ShopCode = s.Shop != null ? s.Shop.Code : "N/A",
            
                    employee_phone_number = s.employee_phone_number,
                    verification_code = s.verification_code
                })
                .ToListAsync(); 
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ShiftsDto>> GetShift(int id)
        {
            var shift = await _context.Shifts
                .Include(s => s.CompanyReason)
                .ThenInclude(cr => cr.Company)
                .Include(s => s.CompanyReason)
                .ThenInclude(cr => cr.Reason)
                .Include(s => s.Shop)
                .FirstOrDefaultAsync(s => s.ShiftID == id);

            if (shift == null)
                return NotFound(); 

            return new ShiftsDto
            {
                ShiftID = shift.ShiftID,
                startDate = shift.start_date,
                EndDate = shift.end_date,
                startTime = shift.start_time,
                endTime = shift.end_time,
                CompanyReasonID = shift.CompanyReasonID,
                CompanyName = shift.CompanyReason?.Company?.CompanyName, 
                ReasonName = shift.CompanyReason?.Reason?.Name, 
                CompanyReason = shift.CompanyReason,
                ShopID = shift.ShopID,
                employee_phone_number = shift.employee_phone_number,
                verification_code = shift.verification_code
            };
        }

        [HttpGet("byreason/{reasonId}")]
        public async Task<ActionResult<IEnumerable<ShiftsDto>>> GetShiftsByReason(int reasonId)
        {
            return await _context.Shifts
                .AsNoTracking()
                .Where(s =>s.CompanyReason.ReasonsReasonID == reasonId)
                .Include(s => s.CompanyReason)
                .ThenInclude(cr => cr.Company)
                .Include(s => s.CompanyReason)
                .ThenInclude(cr => cr.Reason)
                .Select(s => new ShiftsDto
                {
                    ShiftID = s.ShiftID,
                    startDate = s.start_date,
                    EndDate = s.end_date,
                    startTime = s.start_time,
                    endTime = s.end_time,
                    CompanyReasonID = s.CompanyReasonID,
                    CompanyReason = s.CompanyReason,
                    ShopID = s.ShopID,
                    employee_phone_number = s.employee_phone_number
                })
                .ToListAsync();
        }

        [HttpGet("bycompany/{companyId}")]
        public async Task<ActionResult<IEnumerable<ShiftsDto>>> GetShiftsByCompany(int companyId)
        {
            return await _context.Shifts
                .AsNoTracking()
                .Where(s => s.CompanyReason.CompaniesCompanyID == companyId)
                .Include(s => s.Shop)
                .Include(s => s.CompanyReason)
                .ThenInclude(cr => cr.Company)
                .Include(s => s.CompanyReason)
                .ThenInclude(cr => cr.Reason)
                .Select(s => new ShiftsDto
                {
                    ShiftID = s.ShiftID,
                    startDate = s.start_date,
                    EndDate = s.end_date,
                    startTime = s.start_time,
                    endTime = s.end_time,
                    CompanyReasonID = s.CompanyReasonID,
                    CompanyReason = s.CompanyReason,
                    ShopCode = s.Shop != null ? s.Shop.Code : "N/A",
                    Shop = s.Shop,
                    employee_phone_number = s.employee_phone_number,
                    verification_code = s.verification_code
                })
                .ToListAsync();
        }
        
        [HttpGet("byshop/{shopId}")]
        public async Task<ActionResult<IEnumerable<ShiftsDto>>> GetShiftsByShop(int shopId)
        {
            return await _context.Shifts
                .AsNoTracking()
                .Where(s => s.ShopID == shopId)
                .Include(s => s.CompanyReason)
                .ThenInclude(cr => cr.Company)
                .Include(s => s.CompanyReason)
                .ThenInclude(cr => cr.Reason)
                .Select(s => new ShiftsDto
                {
                    ShiftID = s.ShiftID,
                    startDate = s.start_date,
                    EndDate = s.end_date,
                    startTime = s.start_time,
                    endTime = s.end_time,
                    CompanyReasonID = s.CompanyReasonID,
                    CompanyReason = s.CompanyReason,
                    ShopID = s.ShopID,
                    employee_phone_number = s.employee_phone_number,
                    verification_code = s.verification_code
                })
                .ToListAsync();
        }

        [HttpGet("byshoptype/{shopTypeId}")]
        public async Task<ActionResult<IEnumerable<ShiftsDto>>> GetShiftsByShopType(int shopTypeId)
        {
            return await _context.Shifts
                .AsNoTracking()
                .Include(s => s.CompanyReason)
                .ThenInclude(cr => cr.Company)
                .Include(s => s.CompanyReason)
                .ThenInclude(cr => cr.Reason)
                .Where(s => (int)s.Shop.Type == shopTypeId)
                .Select(s => new ShiftsDto
                {
                    ShiftID = s.ShiftID,
                    startDate = s.start_date,
                    EndDate = s.end_date,
                    startTime = s.start_time,
                    endTime = s.end_time,
                    CompanyReasonID = s.CompanyReasonID,
                    CompanyReason = s.CompanyReason,
                    ShopID = s.ShopID,
                    employee_phone_number = s.employee_phone_number,
                    verification_code = s.verification_code
                })
                .ToListAsync();
        }

        [HttpGet("bydate/{date}")]
        public async Task<ActionResult<IEnumerable<ShiftsDto>>> GetShiftsByDate(DateTime date)
        {
            var dateOnly = DateOnly.FromDateTime(date);

            Console.WriteLine($"Looking for shifts on date: {dateOnly}");

            var shifts = await _context.Shifts
                .AsNoTracking()
                .Where(s => s.start_date == dateOnly)
                .Include(s => s.CompanyReason)
                .ThenInclude(cr => cr.Company)
                .Include(s => s.CompanyReason)
                .ThenInclude(cr => cr.Reason)
                .ToListAsync();

            Console.WriteLine($"Found {shifts.Count} shifts");

            return shifts.Select(s => new ShiftsDto
            {
                ShiftID = s.ShiftID,
                startDate = s.start_date,
                EndDate = s.end_date,
                startTime = s.start_time,
                endTime = s.end_time,
                CompanyName = s.CompanyReason?.Company?.CompanyName, 
                ReasonName = s.CompanyReason?.Reason?.Name,
                CompanyReasonID = s.CompanyReasonID,
                CompanyReason = s.CompanyReason,
                ShopID = s.ShopID,
                employee_phone_number = s.employee_phone_number,
                verification_code = s.verification_code
            }).ToList();
        }

        [HttpGet("reminders")]
        public async Task<ActionResult<IEnumerable<ShiftsDto>>> GetShiftReminders()
        {
            var cutoff = DateTime.UtcNow.AddHours(-24);

            return await _context.Shifts    
                .AsNoTracking()
                .Include(s => s.CompanyReason).ThenInclude(cr => cr.Company)
                .Include(s => s.CompanyReason).ThenInclude(cr => cr.Reason)
                .Include(s => s.Shop)
                .Where(s =>
                    (s.start_time == null || s.end_time == null) &&
                    s.start_date != null &&
                    s.start_time != null &&
                    s.start_date.Value.ToDateTime(s.start_time.Value) <= cutoff
                )
                .Select(s => new ShiftsDto
                {
                    ShiftID = s.ShiftID,
                    startDate = s.start_date,
                    startTime = s.start_time, 
                    EndDate = s.end_date, 
                    endTime = s.end_time, 
                    CompanyReasonID = s.CompanyReasonID, 
                    CompanyName = s.CompanyReason != null ?
                    s.CompanyReason.Company.CompanyName : null, 
                    ReasonName = s.CompanyReason != null ? s.CompanyReason.Reason.Name : null, 
                    ShopID = s.ShopID, 
                    ShopCode = s.Shop != null ? s.Shop.Code : "N/A",
                    employee_phone_number = s.employee_phone_number,
                    verification_code = s.verification_code
                })
                .ToListAsync(); 
        }

        [HttpPost]
        public async Task<ActionResult<Shift>> CreateShift(ShiftsDto dto)
        {
            var companyReasonExists = await _context.Set<CompanyReason>()
                .AnyAsync(cr => cr.CompanyReasonID == dto.CompanyReasonID);

            if (!companyReasonExists)
                return BadRequest("Invalid CompanyReasonID");

            var phone = dto.employee_phone_number;
            var shopId = dto.ShopID == 0 ? 1 : dto.ShopID;

            var hasOpenShift = await _context.Shifts
                .AsNoTracking()
                .AnyAsync(s =>
                    s.employee_phone_number == phone &&
                    s.ShopID == shopId &&
                    s.end_date == null &&
                    s.end_time == null
                );

            if (hasOpenShift)
                return Conflict(new { message = "This phone number already has an ongoing shift in this shop." });

            var shift = new Shift
            {
                start_date = dto.startDate,
                start_time = dto.startTime,
                end_date = dto.EndDate,
                end_time = dto.endTime,
                CompanyReasonID = dto.CompanyReasonID,
                ShopID = shopId,
                employee_phone_number = dto.employee_phone_number,
                verification_code = dto.verification_code
            };

            _context.Shifts.Add(shift);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetShift), new { id = shift.ShiftID }, shift);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateShift(int id, ShiftsDto dto)
        {
            var shift = await _context.Shifts.FindAsync(id); 
            if (shift == null)
                return NotFound(); 
            
            if (dto.startTime != null) 
                shift.start_time = dto.startTime; 
            if (dto.endTime != null) 
                shift.end_time = dto.endTime; 

            if (dto.startDate != null) 
                shift.start_date = dto.startDate;
            if (dto.EndDate != null) 
                shift.end_date = dto.EndDate;

            if (dto.endTime != null && shift.end_date == null)
                shift.end_date = shift.start_date; 
            if (dto.startTime != null && shift.start_date == null)
                shift.start_date = shift.end_date;

            if (shift.start_date != null && shift.end_date == null)
                shift.end_date = shift.start_date; 
            if (shift.end_date != null && shift.start_date == null) 
                shift.start_date = shift.end_date; 
            
            shift.CompanyReasonID = dto.CompanyReasonID;
            shift.ShopID = dto.ShopID; 
            shift.employee_phone_number = dto.employee_phone_number; 
            shift.verification_code = dto.verification_code; 

            await _context.SaveChangesAsync();
            return NoContent();  
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchShift(int id, [FromBody] JsonPatchDocument<Shift> patchDoc)
        {
            if (patchDoc == null) return BadRequest();

            var shift = await _context.Shifts.FindAsync(id);
            if (shift == null) return NotFound();

            patchDoc.ApplyTo(shift, ModelState);
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (shift.start_date != null && shift.end_date == null)
                shift.end_date = shift.start_date;

            if (shift.end_date != null && shift.start_date == null)
                shift.start_date = shift.end_date;

            if (shift.start_time.HasValue)
            {
                var t = shift.start_time.Value;
                shift.start_time = new TimeOnly(t.Hour, t.Minute, t.Second);
            }

            if (shift.end_time.HasValue)
            {
                var t = shift.end_time.Value;
                shift.end_time = new TimeOnly(t.Hour, t.Minute, t.Second);
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShift(int id)
        {
            var shift = await _context.Shifts.FindAsync(id); 
            if (shift == null) 
                return NotFound(); 

            _context.Shifts.Remove(shift); 
            await _context.SaveChangesAsync(); 

            return NoContent();
        }

        [HttpGet("companyreason")]
        public async Task<ActionResult> GetCompanyReasonId([FromQuery] int companyId, [FromQuery] int reasonId)
        {
            var companyReason = await _context.CompanyReasons
                .FirstOrDefaultAsync(cr => cr.CompaniesCompanyID == companyId && cr.ReasonsReasonID == reasonId);
            
            if (companyReason == null)
                return NotFound();
            
            return Ok(new { companyReasonID = companyReason.CompanyReasonID });
        }

        [HttpPost("find-ongoing")]
        public async Task<ActionResult<ShiftsDto>> GetOngoingShift([FromBody] OngoingShiftRequest request)
        {
            Console.WriteLine($"Searching for ongoing shift - Phone: {request.PhoneNumber}, ShopId: {request.ShopId}");
            
            if (string.IsNullOrEmpty(request.PhoneNumber))
            {
                return BadRequest(new { message = "Phone number is required" });
            }
            
            var ongoingShift = await _context.Shifts
                .Include(s => s.CompanyReason)
                .ThenInclude(cr => cr.Company)
                .Include(s => s.CompanyReason)
                .ThenInclude(cr => cr.Reason)
                .Include(s => s.Shop)
                .Where(s => 
                    s.employee_phone_number == request.PhoneNumber && 
                    s.ShopID == request.ShopId && 
                    s.end_date == null && 
                    s.end_time == null)
                .FirstOrDefaultAsync();
            
            Console.WriteLine($"Found shift: {ongoingShift?.ShiftID}");
            
            if (ongoingShift == null)
                return NotFound(new { message = "No ongoing shift found" });
            
            return new ShiftsDto
            {
                ShiftID = ongoingShift.ShiftID,
                startDate = ongoingShift.start_date,
                startTime = ongoingShift.start_time,
                EndDate = ongoingShift.end_date,
                endTime = ongoingShift.end_time,
                CompanyReasonID = ongoingShift.CompanyReasonID,
                CompanyName = ongoingShift.CompanyReason?.Company?.CompanyName,
                ReasonName = ongoingShift.CompanyReason?.Reason?.Name,
                ShopID = ongoingShift.ShopID,
                employee_phone_number = ongoingShift.employee_phone_number,
                verification_code = ongoingShift.verification_code
            };
        }

        public class OngoingShiftRequest
        {
            public string PhoneNumber { get; set; }
            public int ShopId { get; set; }
        }

        [HttpPatch("{id}/end")]
        public async Task<IActionResult> EndShift(int id)
        {
            var shift = await _context.Shifts.FindAsync(id);
            if (shift == null)
                return NotFound();
            var now = DateTime.Now;
            shift.end_date = DateOnly.FromDateTime(now);
            shift.end_time = new TimeOnly(now.Hour, now.Minute, now.Second);
            
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private bool ShiftExists(int id)
        {
            return _context.Shifts.Any(r => r.ShiftID == id); 
        }
    }
}