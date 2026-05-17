using BC = BCrypt.Net.BCrypt;

namespace MyApi.Models
{
    public enum Country
    {
        Lithuania = 1,
        Latvia = 2,
        Estonia = 3,
        Baltics = 4
    }

    public class Company
    {
        public int CompanyID { get; set; }
        public string? CompanyName { get; set; }
        public string? PhoneNumber { get; set; }
        public Country Country { get; set; }
        public string? Address { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? Email { get; set; }
        public string PasswordHash { get; set; } = string.Empty; 
        public int? RemID { get; set; }
        public ICollection<CompanyReason> CompanyReasons { get; set; }

        public void SetPassword(string plainTextPassword)
        {
            PasswordHash = BC.HashPassword(plainTextPassword, workFactor: 12);
        }

        public bool VerifyPassword(string plainTextPassword)
        {
            return BC.Verify(plainTextPassword, PasswordHash);
        }
    }
}