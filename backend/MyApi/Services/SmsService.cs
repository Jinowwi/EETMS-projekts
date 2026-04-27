using System.Collections.Concurrent;
using MyApi.Models;

namespace MyApi.Services
{
    public class SmsService {
        private readonly IConfiguration _config;

        private static readonly ConcurrentDictionary<string, OtpRecord> _otpStore = new(); 
        
        public SmsService(IConfiguration config)
        {
            _config = config; 
        }

        private string GenerateOtp()
        {
            return new Random().Next(100000, 999999).ToString(); 
        }

        private static readonly HashSet<string> ValidBalticPrefixes = new()
        {
            // Lithuania +370 - mobile prefixes
            "6",
            // Latvia +371 - mobile prefixes
            "2",
            // Estonia +372 - mobile prefixes
            "5", "81", "82", "83", "84", "85", "87", "89"
        };

        private void ValidatePhoneNumber(string phoneNumber)
        {
            // Strip the country code and get the local part
            string local;
            if (phoneNumber.StartsWith("+370"))
            {
                local = phoneNumber[4..];
                if (local.Length != 8)
                    throw new Exception("INVALID_LENGTH");
                if (!local.StartsWith("6"))
                    throw new Exception("INVALID_NUMBER"); // LT mobiles start with 6
            }
            else if (phoneNumber.StartsWith("+371"))
            {
                local = phoneNumber[4..];
                if (local.Length != 8)
                    throw new Exception("INVALID_LENGTH");
                if (!local.StartsWith("2"))
                    throw new Exception("INVALID_NUMBER"); // LV mobiles start with 2
            }
            else if (phoneNumber.StartsWith("+372"))
            {
                local = phoneNumber[4..];
                if (local.Length < 7 || local.Length > 8)
                    throw new Exception("INVALID_LENGTH");
                // EE mobiles start with 5, or 8x
                bool validPrefix = local.StartsWith("5") ||
                                (local.Length == 8 && local.StartsWith("8") && 
                                    new[] {"81","82","83","84","85","87","89"}
                                    .Any(p => local.StartsWith(p)));
                if (!validPrefix)
                    throw new Exception("INVALID_NUMBER");
            }
            else
            {
                throw new Exception("INVALID_NUMBER");
            }

            // Reject all same digits (00000000, 66666666)
            if (local.Distinct().Count() == 1)
                throw new Exception("INVALID_NUMBER");

            // Reject sequential (56789012 etc.)
            var ascending  = "0123456789";
            var descending = "9876543210";
            if (ascending.Contains(local) || descending.Contains(local))
                throw new Exception("INVALID_NUMBER");
        }

        public async Task<bool> SendOtpAsync(string phoneNumber)
        {
            ValidatePhoneNumber(phoneNumber);
            
            if (_otpStore.TryGetValue(phoneNumber, out var existing))
            {
                if (DateTime.UtcNow < existing.ExpiresAt.AddMinutes(-4))
                    throw new Exception("COOLDOWN"); 
            }

            var otp = GenerateOtp(); 
            var expiresAt = DateTime.UtcNow.AddMinutes(5); 

            var apiKey = _config["SMS_API_KEY"]; 
            var content = Uri.EscapeDataString($"Rimi EETMS verification code: {otp}");
            var url = $"https://direct.traffic.sales.lv/API:0.12/?Command=SendOne&APIKey={apiKey}&Number={Uri.EscapeDataString(phoneNumber)}&Content={content}&Sender=RIMI";

            using var client = new HttpClient(); 
            var response = await client.GetAsync(url); 

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync(); 
                throw new Exception($"SMS service error: {err}");
            }

            _otpStore[phoneNumber] = new OtpRecord { Otp = otp, ExpiresAt = expiresAt };
            return true; 
        }

        public VerifyResult VerifyOtp(string phoneNumber, string enteredOtp)
        {
            if (!_otpStore.TryGetValue(phoneNumber, out var record))
                return new VerifyResult { Valid = false, Reason = "wrong_code" }; 
            
            if (DateTime.UtcNow > record.ExpiresAt)
            {
                _otpStore.TryRemove(phoneNumber, out _);
                return new VerifyResult { Valid = false, Reason = "expired" }; 
            }

            if (record.Otp != enteredOtp)
                return new VerifyResult { Valid = false, Reason = "wrong_code" };

            _otpStore.TryRemove(phoneNumber, out _);
            return new VerifyResult { Valid = true };
        }
    }

    public class OtpRecord
    {
        public string Otp { get; set; } = ""; 
        public DateTime ExpiresAt { get; set; } 
    }

    public class VerifyResult
    {
        public bool Valid { get; set; }
        public string? Reason { get; set; }
    }
}