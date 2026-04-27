namespace MyApi.Dto
{
    public class CompanyDto
    {
        public int CompanyID { get; set; }
        public string? CompanyName { get; set; }
        public string? Address { get; set; }
        public int Country { get; set; }
        public int? RemID { get; set; }
        public string? RimiEmployeeEmail { get; set; }
        public string? PhoneNumber { get; set; }
        public string? RegistrationNumber { get; set; }
        public string? Email { get; set; }
        public string? Password { get; set; }
    }
}