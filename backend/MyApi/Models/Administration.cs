using BC = BCrypt.Net.BCrypt;

namespace MyApi.Models
{
    public class Administration
    {
        public int RemID { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        
        // adminu tipa enum
        public enum Type
        {
            RealEstate = 1,
            Admin = 2,
            SuperAdmin = 3
        }
        public Type TypeOfAdmin { get; set; }

        public string PasswordHash { get; set; } = string.Empty;

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
