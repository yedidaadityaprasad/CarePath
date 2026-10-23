using CarePath.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarePath.Data.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");
        builder.Property(a => a.Reason).HasMaxLength(500).IsRequired();
        builder.Property(a => a.Location).HasMaxLength(100);
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.BookedBy).HasMaxLength(256).IsRequired();
        builder.Property(a => a.CompletionNotes).HasMaxLength(2000);
        builder.Property(a => a.CancellationReason).HasMaxLength(500);
        builder.Ignore(a => a.DurationMinutes);
        builder.HasIndex(a => new { a.PractitionerId, a.StartsAt });

        builder.HasOne(a => a.Patient).WithMany(p => p.Appointments).HasForeignKey(a => a.PatientId);
        builder.HasOne(a => a.Practitioner).WithMany(p => p.Appointments).HasForeignKey(a => a.PractitionerId);
        builder.HasMany(a => a.StatusChanges).WithOne(c => c.Appointment).HasForeignKey(c => c.AppointmentId);
    }
}
