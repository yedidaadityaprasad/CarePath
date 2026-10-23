namespace CarePath.Models.Entities;

public class LabTestDefinition : Entity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int SpecimenTypeId { get; set; }
    public SpecimenType SpecimenType { get; set; } = null!;
    public ResultValueType ResultType { get; set; }
    public string? Unit { get; set; }
    public decimal? ReferenceLow { get; set; }
    public decimal? ReferenceHigh { get; set; }
    public string? ReferenceText { get; set; }
    public decimal Price { get; set; }
    public bool IsActive { get; set; } = true;

    public string ReferenceRangeDisplay => ResultType == ResultValueType.Numeric
        ? $"{ReferenceLow?.ToString("0.###") ?? "-"} to {ReferenceHigh?.ToString("0.###") ?? "-"} {Unit}".Trim()
        : ReferenceText ?? "-";

    public ResultFlag Evaluate(decimal? numericValue, string? textValue)
    {
        if (ResultType == ResultValueType.Numeric && numericValue.HasValue)
        {
            if (ReferenceLow.HasValue && numericValue.Value < ReferenceLow.Value)
            {
                return ResultFlag.Low;
            }
            if (ReferenceHigh.HasValue && numericValue.Value > ReferenceHigh.Value)
            {
                return ResultFlag.High;
            }
            return ResultFlag.Normal;
        }

        if (ResultType == ResultValueType.Text && !string.IsNullOrWhiteSpace(ReferenceText) && textValue != null)
        {
            return string.Equals(ReferenceText.Trim(), textValue.Trim(), StringComparison.OrdinalIgnoreCase)
                ? ResultFlag.Normal
                : ResultFlag.Abnormal;
        }

        return ResultFlag.Normal;
    }
}
