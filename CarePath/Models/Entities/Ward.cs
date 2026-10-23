namespace CarePath.Models.Entities;

public class Ward : Entity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;
    public int Floor { get; set; }
    public decimal DailyRate { get; set; }
    public bool IsClosed { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? ClosureReason { get; set; }

    public ICollection<Bed> Beds { get; set; } = new List<Bed>();
    public ICollection<WardStock> Stock { get; set; } = new List<WardStock>();
    public ICollection<Shift> Shifts { get; set; } = new List<Shift>();
}
