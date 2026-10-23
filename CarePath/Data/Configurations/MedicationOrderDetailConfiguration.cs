using CarePath.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarePath.Data.Configurations;

public class MedicationOrderDetailConfiguration : IEntityTypeConfiguration<MedicationOrderDetail>
{
    public void Configure(EntityTypeBuilder<MedicationOrderDetail> builder)
    {
        builder.ToTable("MedicationOrderDetails");
        builder.Property(d => d.Dose).HasMaxLength(50).IsRequired();
        builder.Property(d => d.Route).HasMaxLength(50).IsRequired();
        builder.Property(d => d.Frequency).HasMaxLength(50).IsRequired();

        builder.HasOne(d => d.Drug).WithMany().HasForeignKey(d => d.DrugId);
    }
}
