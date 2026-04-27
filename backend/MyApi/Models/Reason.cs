namespace MyApi.Models
{
    public class Reason
    {
        public int ReasonID { get; set; }
        public string? Name { get; set; }
        public ICollection<CompanyReason> CompanyReasons { get; set; }
    }
}