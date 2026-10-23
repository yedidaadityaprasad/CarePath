namespace CarePath.Models.Entities;

public class Drug : Entity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int DrugCategoryId { get; set; }
    public DrugCategory Category { get; set; } = null!;
    public string Strength { get; set; } = string.Empty;
    public string Form { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<WardStock> Stock { get; set; } = new List<WardStock>();

    public string DisplayName => $"{Name} {Strength} {Form}";
}
