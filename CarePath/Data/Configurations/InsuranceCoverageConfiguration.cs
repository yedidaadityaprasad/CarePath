using CarePath.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarePath.Data.Configurations;

public class InsuranceCoverageConfiguration : IEntityTypeConfiguration<InsuranceCoverage>
{
    public void Configure(EntityTypeBuilder<InsuranceCoverage> builder)
    {
        builder.ToTable("InsuranceCoverages");
        builder.Property(c => c.MemberId).HasMaxLength(50).IsRequired();
        builder.Property(c => c.GroupNumber).HasMaxLength(50);
        builder.Property(c => c.CoveragePercent).HasPrecision(5, 2);
        builder.Property(c => c.TerminationReason).HasMaxLength(500);
        builder.HasIndex(c => new { c.PatientId, c.PayerId, c.IsActive });

        builder.HasOne(c => c.Patient).WithMany(p => p.Coverages).HasForeignKey(c => c.PatientId);
        builder.HasOne(c => c.Payer).WithMany(p => p.Coverages).HasForeignKey(c => c.PayerId);
    }
}
