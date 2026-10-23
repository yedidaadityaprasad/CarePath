namespace CarePath.Models.Entities;

public class Invoice : Entity
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public int AdmissionId { get; set; }
    public Admission Admission { get; set; } = null!;
    public int PatientId { get; set; }
    public Patient Patient { get; set; } = null!;
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Draft;
    public DateTime CreatedAt { get; set; }
    public DateTime? IssuedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime? VoidedAt { get; set; }
    public string? VoidReason { get; set; }
    public int RoomDays { get; set; }
    public decimal SubTotal { get; set; }
    public decimal CoveredAmount { get; set; }
    public decimal PatientResponsibility { get; set; }

    public ICollection<InvoiceLine> Lines { get; set; } = new List<InvoiceLine>();
    public InvoiceCoverageSnapshot? CoverageSnapshot { get; set; }
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<InvoiceStatusChange> StatusChanges { get; set; } = new List<InvoiceStatusChange>();

    public decimal AmountPaid => Payments.Sum(p => p.Amount);
    public decimal BalanceDue => PatientResponsibility - AmountPaid;

    public void ChangeStatus(InvoiceStatus newStatus, DateTime at, string changedBy, string? note)
    {
        StatusChanges.Add(new InvoiceStatusChange
        {
            Invoice = this,
            FromStatus = Status,
            ToStatus = newStatus,
            ChangedAt = at,
            ChangedBy = changedBy,
            Note = note
        });
        Status = newStatus;
    }
}
