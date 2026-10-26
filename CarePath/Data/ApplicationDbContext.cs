using CarePath.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CarePath.Data;

/// <summary>
/// Main EF Core DbContext for CarePath.
/// .NET 10 / EF Core 10: Uses IdentityDbContext with ApplicationUser.
/// All domain foreign keys use DeleteBehavior.Restrict to prevent accidental cascade deletes
/// across clinical and financial history tables.
/// </summary>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<PatientAddress> PatientAddresses => Set<PatientAddress>();
    public DbSet<EmergencyContact> EmergencyContacts => Set<EmergencyContact>();
    public DbSet<PatientAllergy> PatientAllergies => Set<PatientAllergy>();
    public DbSet<Payer> Payers => Set<Payer>();
    public DbSet<InsuranceCoverage> InsuranceCoverages => Set<InsuranceCoverage>();
    public DbSet<Specialty> Specialties => Set<Specialty>();
    public DbSet<Practitioner> Practitioners => Set<Practitioner>();
    public DbSet<PractitionerStatusChange> PractitionerStatusChanges => Set<PractitionerStatusChange>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Ward> Wards => Set<Ward>();
    public DbSet<Bed> Beds => Set<Bed>();
    public DbSet<BedStatusChange> BedStatusChanges => Set<BedStatusChange>();
    public DbSet<Admission> Admissions => Set<Admission>();
    public DbSet<BedAssignment> BedAssignments => Set<BedAssignment>();
    public DbSet<AdmissionDiagnosis> AdmissionDiagnoses => Set<AdmissionDiagnosis>();
    public DbSet<AdmissionNote> AdmissionNotes => Set<AdmissionNote>();
    public DbSet<VitalSign> VitalSigns => Set<VitalSign>();
    public DbSet<DischargeSummary> DischargeSummaries => Set<DischargeSummary>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AppointmentStatusChange> AppointmentStatusChanges => Set<AppointmentStatusChange>();
    public DbSet<ClinicalOrder> ClinicalOrders => Set<ClinicalOrder>();
    public DbSet<OrderStatusChange> OrderStatusChanges => Set<OrderStatusChange>();
    public DbSet<LabTestDefinition> LabTestDefinitions => Set<LabTestDefinition>();
    public DbSet<SpecimenType> SpecimenTypes => Set<SpecimenType>();
    public DbSet<LabOrderDetail> LabOrderDetails => Set<LabOrderDetail>();
    public DbSet<Specimen> Specimens => Set<Specimen>();
    public DbSet<LabResult> LabResults => Set<LabResult>();
    public DbSet<DrugCategory> DrugCategories => Set<DrugCategory>();
    public DbSet<Drug> Drugs => Set<Drug>();
    public DbSet<WardStock> WardStocks => Set<WardStock>();
    public DbSet<MedicationOrderDetail> MedicationOrderDetails => Set<MedicationOrderDetail>();
    public DbSet<DispenseRecord> DispenseRecords => Set<DispenseRecord>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
    public DbSet<InvoiceCoverageSnapshot> InvoiceCoverageSnapshots => Set<InvoiceCoverageSnapshot>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<InvoiceStatusChange> InvoiceStatusChanges => Set<InvoiceStatusChange>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Clinical and financial history is never hard-deleted, and SQL Server rejects
        // multiple cascade paths, so every domain foreign key restricts deletes.
        var domainForeignKeys = builder.Model.GetEntityTypes()
            .Where(t => typeof(Entity).IsAssignableFrom(t.ClrType))
            .SelectMany(t => t.GetForeignKeys());

        foreach (var foreignKey in domainForeignKeys)
        {
            foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
        }
    }

    // .NET 10 / EF Core 10: Override SaveChangesAsync to support future interceptors or
    // audit hooks without changing the service layer.
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => base.SaveChangesAsync(cancellationToken);
}
