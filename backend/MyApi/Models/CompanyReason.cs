using MyApi.Models;

// starptabula iemeslu piešķiršanai uzņēmumiem
namespace MyApi.Models
{
    public class CompanyReason
    {
        public int CompanyReasonID { get; set; }
        public int CompaniesCompanyID { get; set; }
        public Company? Company { get; set; }
        public int ReasonsReasonID { get; set; }
        public Reason? Reason { get; set; }
    }
}