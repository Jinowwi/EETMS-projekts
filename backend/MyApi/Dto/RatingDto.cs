using System.ComponentModel.DataAnnotations;

public class RatingDto
{
    [Range(1, 5)]
    public int Stars { get; set; }

    [MaxLength(1000)]
    public string? Comment { get; set; }

    public int? ShiftRequestID { get; set; }
}