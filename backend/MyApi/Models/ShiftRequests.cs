namespace MyApi.Models
{
    public enum ShiftRequestStatus
    {
        Sent = 1,
        Approved = 2,
        InProgress = 3,
        Done = 4
    }

    public class ShiftRequest
    {
        public int ShiftRequestID { get; set; }
        public string Description { get; set; } = string.Empty;
        public ShiftRequestStatus Status { get; set; }
        public int ReasonID { get; set; }
        public Reason Reason { get; set; }
        public int? RemId { get; set; }
        public Administration? Rem { get; set; }
        public int? CompanyId { get; set; }
        public Company? Company { get; set; }
        public int ShopId { get; set; }
        public Shop Shop { get; set; }
        public PlannedShifts? PlannedShift { get; set; }
        public Ratings? Rating { get; set; }
    }
}