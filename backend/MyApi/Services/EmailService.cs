using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace MyApi.Services
{
    public class EmailService
    {
        private readonly EmailSettings _settings;
        private readonly HttpClient _http;

        public EmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
            _http = new HttpClient();
        }

        public async Task SendPasswordSetupEmailAsync(string toEmail, string shopCode, string token)
        {
            var setupLink = $"http://localhost:5173/set-password?token={token}&email={toEmail}";

            var payload = new
            {
                sender = new { name = _settings.FromName, email = _settings.FromEmail },
                to = new[] { new { email = toEmail, name = shopCode } },
                subject = "Set up your shop password",
                htmlContent = $"""
                    <div style="font-family: Inter, sans-serif; max-width: 480px; margin: 0 auto;">
                        <h2 style="color: #a12971;">Welcome, {shopCode}!</h2>
                        <p>Your shop account has been created. Click the button below to set your password.</p>
                        <a href="{setupLink}"
                           style="display:inline-block; padding:12px 24px; background:#a12971;
                                  color:#fff; border-radius:24px; text-decoration:none;
                                  font-weight:600; margin: 16px 0;">
                            Set Password
                        </a>
                        <p style="color:#999; font-size:12px;">
                            This link expires in 24 hours. If you didn't expect this email, ignore it.
                        </p>
                    </div>
                """
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.brevo.com/v3/smtp/email");
            request.Headers.Add("api-key", _settings.BrevoApiKey);
            request.Content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _http.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Brevo API error: {body}");
        }
    }
}