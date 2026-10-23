using CarePath.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarePath.Data.Configurations;

public class AppointmentStatusChangeConfiguration : IEntityTypeConfiguration<AppointmentStatusChange>
{
    public void Configure(EntityTypeBuilder<AppointmentStatusChange> builder)
    {
        builder.ToTable("AppointmentStatusChanges");
        builder.Property(c => c.FromStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.ToStatus).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.ChangedBy).HasMaxLength(256).IsRequired();
        builder.Property(c => c.Note).HasMaxLength(500);
    }
}
