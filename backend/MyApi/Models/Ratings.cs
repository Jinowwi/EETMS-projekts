namespace MyApi.Models
{
    public class Ratings
    {
        public int RatingID { get; set; }
        public int Stars { get; set; }
        public string? Comment { get; set; }
        public int ShiftRequestID { get; set; }
        public ShiftRequest ShiftRequest { get; set; }
    }
}