using CarePath.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarePath.Data.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.ToTable("Patients");
        builder.Property(p => p.MedicalRecordNumber).HasMaxLength(20).IsRequired();
        builder.HasIndex(p => p.MedicalRecordNumber).IsUnique();
        builder.Property(p => p.FirstName).HasMaxLength(80).IsRequired();
        builder.Property(p => p.LastName).HasMaxLength(80).IsRequired();
        builder.HasIndex(p => new { p.LastName, p.FirstName });
        builder.Property(p => p.Gender).HasConversion<string>().HasMaxLength(10);
        builder.Property(p => p.Phone).HasMaxLength(30);
        builder.Property(p => p.Email).HasMaxLength(200);
        builder.Property(p => p.DeactivationReason).HasMaxLength(500);
        builder.Ignore(p => p.FullName);

        builder.HasOne(p => p.Address)
            .WithOne(a => a.Patient)
            .HasForeignKey<PatientAddress>(a => a.PatientId);
        builder.HasMany(p => p.EmergencyContacts).WithOne(c => c.Patient).HasForeignKey(c => c.PatientId);
        builder.HasMany(p => p.Allergies).WithOne(a => a.Patient).HasForeignKey(a => a.PatientId);
    }
}
