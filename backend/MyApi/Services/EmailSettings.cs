namespace MyApi.Services
{
    public class EmailSettings
    {
        public string BrevoApiKey { get; set; } = string.Empty;
        public string FromName { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
    }
}