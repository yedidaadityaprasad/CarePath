namespace CarePath.Models.Entities;

public class Specialty : Entity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public ICollection<Practitioner> Practitioners { get; set; } = new List<Practitioner>();
}
