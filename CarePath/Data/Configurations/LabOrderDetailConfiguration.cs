using CarePath.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarePath.Data.Configurations;

public class LabOrderDetailConfiguration : IEntityTypeConfiguration<LabOrderDetail>
{
    public void Configure(EntityTypeBuilder<LabOrderDetail> builder)
    {
        builder.ToTable("LabOrderDetails");
        builder.Property(d => d.ClinicalIndication).HasMaxLength(500);

        builder.HasOne(d => d.TestDefinition).WithMany().HasForeignKey(d => d.LabTestDefinitionId);
        builder.HasOne(d => d.Specimen).WithOne(s => s.LabOrder).HasForeignKey<Specimen>(s => s.LabOrderDetailId);
        builder.HasOne(d => d.Result).WithOne(r => r.LabOrder).HasForeignKey<LabResult>(r => r.LabOrderDetailId);
    }
}
