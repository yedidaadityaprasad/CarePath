using CarePath.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CarePath.Data.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");
        builder.Property(i => i.InvoiceNumber).HasMaxLength(30).IsRequired();
        builder.HasIndex(i => i.InvoiceNumber).IsUnique();
        builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(i => i.Status);
        builder.Property(i => i.VoidReason).HasMaxLength(500);
        builder.Property(i => i.SubTotal).HasPrecision(18, 2);
        builder.Property(i => i.CoveredAmount).HasPrecision(18, 2);
        builder.Property(i => i.PatientResponsibility).HasPrecision(18, 2);
        builder.Ignore(i => i.AmountPaid);
        builder.Ignore(i => i.BalanceDue);

        builder.HasOne(i => i.Patient).WithMany().HasForeignKey(i => i.PatientId);
        builder.HasMany(i => i.Lines).WithOne(l => l.Invoice).HasForeignKey(l => l.InvoiceId);
        builder.HasOne(i => i.CoverageSnapshot).WithOne(s => s.Invoice).HasForeignKey<InvoiceCoverageSnapshot>(s => s.InvoiceId);
        builder.HasMany(i => i.Payments).WithOne(p => p.Invoice).HasForeignKey(p => p.InvoiceId);
        builder.HasMany(i => i.StatusChanges).WithOne(c => c.Invoice).HasForeignKey(c => c.InvoiceId);
    }
}
