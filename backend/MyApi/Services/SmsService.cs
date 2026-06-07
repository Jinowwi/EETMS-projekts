using System.Collections.Concurrent;
using MyApi.Models;

namespace MyApi.Services
{
    public class SmsService
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpFactory;

        private static readonly ConcurrentDictionary<string, DateTime> _cooldowns = new();

        public SmsService(IConfiguration config, IHttpClientFactory httpFactory)
        {
            _config = config;
            _httpFactory = httpFactory;
        }

        private static readonly HashSet<string> ValidBalticPrefixes = new()
        {
            "6", "2", "5", "81", "82", "83", "84", "85", "87", "89"
        };

        private void ValidatePhoneNumber(string phoneNumber)
        {
            string local;
            if (phoneNumber.StartsWith("+370"))
            {
                local = phoneNumber[4..];
                if (local.Length != 8) throw new Exception("INVALID_LENGTH");
                if (!local.StartsWith("6")) throw new Exception("INVALID_NUMBER");
            }
            else if (phoneNumber.StartsWith("+371"))
            {
                local = phoneNumber[4..];
                if (local.Length != 8) throw new Exception("INVALID_LENGTH");
                if (!local.StartsWith("2")) throw new Exception("INVALID_NUMBER");
            }
            else if (phoneNumber.StartsWith("+372"))
            {
                local = phoneNumber[4..];
                if (local.Length < 7 || local.Length > 8) throw new Exception("INVALID_LENGTH");
                bool validPrefix = local.StartsWith("5") ||
                    (local.Length == 8 && local.StartsWith("8") &&
                        new[] { "81","82","83","84","85","87","89" }
                        .Any(p => local.StartsWith(p)));
                if (!validPrefix) throw new Exception("INVALID_NUMBER");
            }
            else
            {
                throw new Exception("INVALID_NUMBER");
            }

            if (local.Distinct().Count() == 1) throw new Exception("INVALID_NUMBER");
            var ascending = "0123456789";
            var descending = "9876543210";
            if (ascending.Contains(local) || descending.Contains(local))
                throw new Exception("INVALID_NUMBER");
        }

        public async Task<bool> SendOtpAsync(string phoneNumber)
        {
            ValidatePhoneNumber(phoneNumber);

            if (_cooldowns.TryGetValue(phoneNumber, out var lastSent))
                if (DateTime.UtcNow < lastSent.AddMinutes(1))
                    throw new Exception("COOLDOWN");

            var accountSid = _config["TWILIO_ACCOUNT_SID"];
            var authToken  = _config["TWILIO_AUTH_TOKEN"];
            var serviceSid = _config["TWILIO_VERIFY_SERVICE_SID"];

            var credentials = Convert.ToBase64String(
                System.Text.Encoding.ASCII.GetBytes($"{accountSid}:{authToken}"));

            var body = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["To"]      = phoneNumber,
                ["Channel"] = "sms"
            });

            var client = _httpFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

            var url = $"https://verify.twilio.com/v2/Services/{serviceSid}/Verifications";
            var response = await client.PostAsync(url, body);

            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                throw new Exception($"SMS service error: {err}");
            }

            _cooldowns[phoneNumber] = DateTime.UtcNow;
            return true;
        }

        public async Task<VerifyResult> VerifyOtpAsync(string phoneNumber, string enteredOtp)
        {
            var accountSid = _config["TWILIO_ACCOUNT_SID"];
            var authToken  = _config["TWILIO_AUTH_TOKEN"];
            var serviceSid = _config["TWILIO_VERIFY_SERVICE_SID"];

            var credentials = Convert.ToBase64String(
                System.Text.Encoding.ASCII.GetBytes($"{accountSid}:{authToken}"));

            var body = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["To"]   = phoneNumber,
                ["Code"] = enteredOtp
            });

            var client = _httpFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

            var url = $"https://verify.twilio.com/v2/Services/{serviceSid}/VerificationCheck";
            var response = await client.PostAsync(url, body);
            var json = await response.Content.ReadAsStringAsync();

            if (json.Contains("\"approved\""))
                return new VerifyResult { Valid = true };

            if (json.Contains("expired"))
                return new VerifyResult { Valid = false, Reason = "expired" };

            return new VerifyResult { Valid = false, Reason = "wrong_code" };
        }
    }

    public class VerifyResult
    {
        public bool Valid { get; set; }
        public string? Reason { get; set; }
    }
}