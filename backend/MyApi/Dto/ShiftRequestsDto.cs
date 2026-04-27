public class ShiftRequestDto
{
    public string Description { get; set; } = string.Empty;
    public int ReasonID { get; set; }
    public int ShopId { get; set; }
    public int? CompanyId { get; set; }
}