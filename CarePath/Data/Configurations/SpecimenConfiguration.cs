using CarePath.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarePath.Data.Configurations;

public class SpecimenConfiguration : IEntityTypeConfiguration<Specimen>
{
    public void Configure(EntityTypeBuilder<Specimen> builder)
    {
        builder.ToTable("Specimens");
        builder.Property(s => s.AccessionNumber).HasMaxLength(30).IsRequired();
        builder.HasIndex(s => s.AccessionNumber).IsUnique();
        builder.Property(s => s.CollectedBy).HasMaxLength(256).IsRequired();
        builder.Property(s => s.Notes).HasMaxLength(500);

        builder.HasOne(s => s.SpecimenType).WithMany().HasForeignKey(s => s.SpecimenTypeId);
    }
}
