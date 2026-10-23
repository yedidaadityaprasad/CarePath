namespace CarePath.Models.Entities;

public class Patient : Entity
{
    public string MedicalRecordNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime RegisteredAt { get; set; }
    public DateTime? DeactivatedAt { get; set; }
    public string? DeactivationReason { get; set; }

    public PatientAddress? Address { get; set; }
    public ICollection<EmergencyContact> EmergencyContacts { get; set; } = new List<EmergencyContact>();
    public ICollection<PatientAllergy> Allergies { get; set; } = new List<PatientAllergy>();
    public ICollection<InsuranceCoverage> Coverages { get; set; } = new List<InsuranceCoverage>();
    public ICollection<Admission> Admissions { get; set; } = new List<Admission>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public string FullName => $"{LastName}, {FirstName}";

    public int AgeOn(DateTime date)
    {
        var age = date.Year - DateOfBirth.Year;
        if (DateOfBirth.Date > date.Date.AddYears(-age))
        {
            age--;
        }
        return Math.Max(age, 0);
    }
}
