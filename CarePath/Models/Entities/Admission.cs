namespace CarePath.Models.Entities;

public class Admission : Entity
{
    public string AdmissionNumber { get; set; } = string.Empty;
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public int AttendingPractitionerId { get; set; }
    public Practitioner AttendingPractitioner { get; set; } = null!;
    public int CurrentBedId { get; set; }
    public Bed CurrentBed { get; set; } = null!;
    public int CoverageId { get; set; }
    public InsuranceCoverage Coverage { get; set; } = null!;
    public AdmissionStatus Status { get; set; } = AdmissionStatus.Open;
    public DateTime AdmittedAt { get; set; }
    public DateTime? DischargedAt { get; set; }
    public string ReasonForAdmission { get; set; } = string.Empty;
    public string AdmittedBy { get; set; } = string.Empty;

    public ICollection<BedAssignment> BedAssignments { get; set; } = new List<BedAssignment>();
    public ICollection<AdmissionDiagnosis> Diagnoses { get; set; } = new List<AdmissionDiagnosis>();
    public ICollection<AdmissionNote> Notes { get; set; } = new List<AdmissionNote>();
    public ICollection<VitalSign> Vitals { get; set; } = new List<VitalSign>();
    public ICollection<ClinicalOrder> Orders { get; set; } = new List<ClinicalOrder>();
    public DischargeSummary? DischargeSummary { get; set; }
    public Invoice? Invoice { get; set; }

    public bool IsOpen => Status == AdmissionStatus.Open;

    public int LengthOfStayDays(DateTime asOf)
    {
        var end = DischargedAt ?? asOf;
        var days = (end.Date - AdmittedAt.Date).Days;
        return Math.Max(days, 1);
    }
}
