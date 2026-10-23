using CarePath.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarePath.Data.Configurations;

public class InvoiceLineConfiguration : IEntityTypeConfiguration<InvoiceLine>
{
    public void Configure(EntityTypeBuilder<InvoiceLine> builder)
    {
        builder.ToTable("InvoiceLines");
        builder.Property(l => l.LineType).HasConversion<string>().HasMaxLength(30);
        builder.Property(l => l.Description).HasMaxLength(300).IsRequired();
        builder.Property(l => l.Quantity).HasPrecision(18, 2);
        builder.Property(l => l.UnitPrice).HasPrecision(18, 2);
        builder.Property(l => l.Amount).HasPrecision(18, 2);

        builder.HasOne(l => l.ClinicalOrder).WithMany().HasForeignKey(l => l.ClinicalOrderId);
    }
}
