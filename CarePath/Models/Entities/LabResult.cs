namespace CarePath.Models.Entities;

public class LabResult : Entity
{
    public int LabOrderDetailId { get; set; }
    public LabOrderDetail LabOrder { get; set; } = null!;
    public decimal? NumericValue { get; set; }
    public string? TextValue { get; set; }
    public ResultFlag Flag { get; set; }
    public string? Comment { get; set; }
    public DateTime ResultedAt { get; set; }
    public string ResultedBy { get; set; } = string.Empty;

    public string DisplayValue => NumericValue?.ToString("0.###") ?? TextValue ?? string.Empty;
}
