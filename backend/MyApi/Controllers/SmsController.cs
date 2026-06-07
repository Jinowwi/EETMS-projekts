using Microsoft.AspNetCore.Mvc; 
using MyApi.Services; 

namespace MyApi.Controllers
{
    [ApiController]
    [Route("api/sms")]
    public class SmsController : ControllerBase
    {
        private readonly SmsService _smsService; 

        public SmsController(SmsService smsService)
        {
            _smsService = smsService; 
        }

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] SendOtpRequest req)
        {
            if (string.IsNullOrEmpty(req.PhoneNumber))
                return BadRequest(new { error = "Phone number required" });

            try
            {
                await _smsService.SendOtpAsync(req.PhoneNumber);
                return Ok(new { success = true });
            }
            catch (Exception ex) when (ex.Message == "COOLDOWN")
            {
                return StatusCode(429, new { error = "Please wait before requesting a new code." });
            }
            catch (Exception ex)
            {
                return StatusCode(502, new { error = ex.Message });
            }
        }

        [HttpPost("verify")]
        public async Task<IActionResult> Verify([FromBody] VerifyOtpRequest req)
        {
            if (string.IsNullOrEmpty(req.PhoneNumber) || string.IsNullOrEmpty(req.Otp))
                return BadRequest(new { error = "Missing fields" });

            var result = await _smsService.VerifyOtpAsync(req.PhoneNumber, req.Otp);

            if (!result.Valid)
                return BadRequest(new { error = result.Reason });

            return Ok(new { valid = true });
        }
    }

    public class SendOtpRequest { 
        public string PhoneNumber { get; set; } = ""; 
    }

    public class VerifyOtpRequest { 
        public string PhoneNumber { get; set; } = ""; 
        public string Otp { get; set; } = ""; 
    }
}