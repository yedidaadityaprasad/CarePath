namespace CarePath.Models.Entities;

public class Practitioner : Entity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public PractitionerType Type { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public int? SpecialtyId { get; set; }
    public Specialty? Specialty { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public ICollection<PractitionerStatusChange> StatusChanges { get; set; } = new List<PractitionerStatusChange>();
    public ICollection<Shift> Shifts { get; set; } = new List<Shift>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public string DisplayName => Type == PractitionerType.Physician
        ? $"Dr. {FirstName} {LastName}"
        : $"{FirstName} {LastName}, RN";
}
