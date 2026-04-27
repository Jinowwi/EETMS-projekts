namespace MyApi.Dto
{
    public class EmployeeListDto
    {
        public int EmployeeID { get; set; }
        public string? PhoneNumber { get; set; }

        // public int CompanyID { get; set; }
        public int Code { get; set; }
        public string? CompanyName { get; set; }
        public int ShiftID { get; set; }
    }
}