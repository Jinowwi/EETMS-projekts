using System.Data;

namespace MyApi.Models
{
    public class PlannedShifts
    {
        public int PlannedShiftsID { get; set; }
        public TimeOnly? approx_start_time { get; set; }
        public DateOnly? approx_date { get; set; }
        public int approx_duration { get; set; }
        public int ShiftRequestID { get; set; }
        public ShiftRequest ShiftRequest { get; set; }
    }
}