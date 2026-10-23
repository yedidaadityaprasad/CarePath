using CarePath.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarePath.Data.Configurations;

public class AdmissionDiagnosisConfiguration : IEntityTypeConfiguration<AdmissionDiagnosis>
{
    public void Configure(EntityTypeBuilder<AdmissionDiagnosis> builder)
    {
        builder.ToTable("AdmissionDiagnoses");
        builder.Property(d => d.Code).HasMaxLength(20).IsRequired();
        builder.Property(d => d.Description).HasMaxLength(300).IsRequired();
        builder.Property(d => d.RecordedBy).HasMaxLength(256).IsRequired();
    }
}
