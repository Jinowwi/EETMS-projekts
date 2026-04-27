namespace MyApi.Models
{
    public class Shift
    {
        public int ShiftID { get; set; }
        public DateOnly? start_date { get; set; }
        public TimeOnly? start_time { get; set; }
        public DateOnly? end_date { get; set; }
        public TimeOnly? end_time { get; set; }
        public int? CompanyReasonID { get; set; }
        public CompanyReason CompanyReason { get; set; } = null!;
        public int ShopID { get; set; }
        public Shop? Shop { get; set; }
        // public Reminder? Reminder { get; set; }
        public string? employee_phone_number { get; set; }
        public string? verification_code { get; set; }
    }
}
