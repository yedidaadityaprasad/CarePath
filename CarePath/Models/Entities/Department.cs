namespace CarePath.Models.Entities;

public class Department : Entity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public ICollection<Ward> Wards { get; set; } = new List<Ward>();
}
