using CarePath.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarePath.Data.Configurations;

public class AdmissionConfiguration : IEntityTypeConfiguration<Admission>
{
    public void Configure(EntityTypeBuilder<Admission> builder)
    {
        builder.ToTable("Admissions");
        builder.Property(a => a.AdmissionNumber).HasMaxLength(30).IsRequired();
        builder.HasIndex(a => a.AdmissionNumber).IsUnique();
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(a => a.Status);
        builder.Property(a => a.ReasonForAdmission).HasMaxLength(500).IsRequired();
        builder.Property(a => a.AdmittedBy).HasMaxLength(256).IsRequired();
        builder.Ignore(a => a.IsOpen);

        builder.HasOne(a => a.Patient).WithMany(p => p.Admissions).HasForeignKey(a => a.PatientId);
        builder.HasOne(a => a.AttendingPractitioner).WithMany().HasForeignKey(a => a.AttendingPractitionerId);
        builder.HasOne(a => a.CurrentBed).WithMany().HasForeignKey(a => a.CurrentBedId);
        builder.HasOne(a => a.Coverage).WithMany().HasForeignKey(a => a.CoverageId);
        builder.HasMany(a => a.BedAssignments).WithOne(b => b.Admission).HasForeignKey(b => b.AdmissionId);
        builder.HasMany(a => a.Diagnoses).WithOne(d => d.Admission).HasForeignKey(d => d.AdmissionId);
        builder.HasMany(a => a.Notes).WithOne(n => n.Admission).HasForeignKey(n => n.AdmissionId);
        builder.HasMany(a => a.Vitals).WithOne(v => v.Admission).HasForeignKey(v => v.AdmissionId);
        builder.HasMany(a => a.Orders).WithOne(o => o.Admission).HasForeignKey(o => o.AdmissionId);
        builder.HasOne(a => a.DischargeSummary).WithOne(d => d.Admission).HasForeignKey<DischargeSummary>(d => d.AdmissionId);
        builder.HasOne(a => a.Invoice).WithOne(i => i.Admission).HasForeignKey<Invoice>(i => i.AdmissionId);
    }
}
