using CarePath.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarePath.Data.Configurations;

public class PractitionerConfiguration : IEntityTypeConfiguration<Practitioner>
{
    public void Configure(EntityTypeBuilder<Practitioner> builder)
    {
        builder.ToTable("Practitioners");
        builder.Property(p => p.FirstName).HasMaxLength(80).IsRequired();
        builder.Property(p => p.LastName).HasMaxLength(80).IsRequired();
        builder.Property(p => p.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.LicenseNumber).HasMaxLength(30).IsRequired();
        builder.HasIndex(p => p.LicenseNumber).IsUnique();
        builder.Property(p => p.Email).HasMaxLength(200);
        builder.Property(p => p.Phone).HasMaxLength(30);
        builder.Ignore(p => p.DisplayName);

        builder.HasOne(p => p.Specialty).WithMany(s => s.Practitioners).HasForeignKey(p => p.SpecialtyId);
        builder.HasMany(p => p.StatusChanges).WithOne(c => c.Practitioner).HasForeignKey(c => c.PractitionerId);
    }
}
