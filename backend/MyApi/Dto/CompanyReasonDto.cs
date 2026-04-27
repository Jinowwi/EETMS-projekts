namespace MyApi.Dto
{
    public class CompanyReasonDto
    {
        public int CompanyReasonID { get; set; }
        public int CompaniesCompanyID { get; set; }
        public CompanyDto Company { get; set; }
        public int ReasonsReasonID { get; set; }
        public ReasonsDto Reason { get; set; }

        public class UpdateCompanyReasonDto
        {
            public int CompanyReasonID { get; set; }
        }
    }
}