namespace CarePath.Models.Entities;

public class DrugCategory : Entity
{
    public string Name { get; set; } = string.Empty;

    public ICollection<Drug> Drugs { get; set; } = new List<Drug>();
}
