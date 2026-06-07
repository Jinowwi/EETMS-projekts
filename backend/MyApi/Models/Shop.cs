using System.Collections.Generic;
using BC = BCrypt.Net.BCrypt;

namespace MyApi.Models
{
    // veikalu tipu enum
    public enum ShopType
    {
        Hyper = 1, 
        Super = 2, 
        Mini = 3, 
        Express = 4
    }
    public class Shop
    {
        public int ShopID { get; set; }
        public string? Code { get; set; }
        public ShopType Type { get; set; }
        public Country Country { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string PasswordHash { get; set; } = string.Empty;
        public List<Shift> Shifts { get; set; } = new();

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