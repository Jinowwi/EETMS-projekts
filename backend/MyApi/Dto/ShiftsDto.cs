using MyApi.Models;

namespace MyApi.Dto
{
    public class ShiftsDto
    {
        public int ShiftID { get; set; }
        public DateOnly? startDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public TimeOnly? startTime { get; set; }
        public TimeOnly? endTime { get; set; }
        public int? CompanyReasonID { get; set; }
        public CompanyReason? CompanyReason { get; set; }
        public string? CompanyName { get; set; }
        public string? ReasonName { get; set; }
        public int ShopID { get; set; }
        public Shop? Shop { get; set; }
        public string? ShopCode { get; set; }
        public string? employee_phone_number { get; set; }
        public string? verification_code { get; set; }
    }
}
