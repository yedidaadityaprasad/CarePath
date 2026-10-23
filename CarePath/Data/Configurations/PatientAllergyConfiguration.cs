using CarePath.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarePath.Data.Configurations;

public class PatientAllergyConfiguration : IEntityTypeConfiguration<PatientAllergy>
{
    public void Configure(EntityTypeBuilder<PatientAllergy> builder)
    {
        builder.ToTable("PatientAllergies");
        builder.Property(a => a.Substance).HasMaxLength(150).IsRequired();
        builder.Property(a => a.Reaction).HasMaxLength(300);
        builder.Property(a => a.Severity).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.RecordedBy).HasMaxLength(256).IsRequired();
    }
}
