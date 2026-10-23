using CarePath.Data;
using CarePath.Exceptions;
using CarePath.Infrastructure;
using CarePath.Models.Entities;
using CarePath.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CarePath.Services;

public class BillingService : IBillingService
{
    private readonly ApplicationDbContext _db;
    private readonly IClock _clock;
    private readonly ICurrentUser _currentUser;
    private readonly IAuditService _audit;

    public BillingService(ApplicationDbContext db, IClock clock, ICurrentUser currentUser, IAuditService audit)
    {
        _db = db;
        _clock = clock;
        _currentUser = currentUser;
        _audit = audit;
    }

    /// <summary>Ward daily rate times calendar days in hospital, with a minimum of one day.</summary>
    public static (int Days, decimal Amount) CalculateRoomCharge(decimal dailyRate, DateTime admittedAt, DateTime dischargedAt)
    {
        var days = Math.Max((dischargedAt.Date - admittedAt.Date).Days, 1);
        return (days, Math.Round(dailyRate * days, 2, MidpointRounding.AwayFromZero));
    }

    public Task<PagedResult<Invoice>> SearchAsync(InvoiceStatus? status, string? search, int page, int pageSize = 25)
    {
        var query = _db.Invoices.AsNoTracking()
            .Include(i => i.Patient)
            .Include(i => i.Admission)
            .Include(i => i.Payments)
            .AsQueryable();
        if (status.HasValue)
        {
            query = query.Where(i => i.Status == status.Value);
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            var upper = term.ToUpperInvariant();
            query = query.Where(i =>
                i.InvoiceNumber.StartsWith(upper) ||
                i.Patient.MedicalRecordNumber.StartsWith(upper) ||
                i.Patient.LastName.Contains(term) ||
                i.Patient.FirstName.Contains(term));
        }
        return query.OrderByDescending(i => i.CreatedAt).ToPagedResultAsync(page, pageSize);
    }

    public async Task<InvoiceDetailsViewModel> GetDetailsAsync(int id)
    {
        var invoice = await _db.Invoices.AsNoTracking()
            .Include(i => i.Patient).ThenInclude(p => p.Address)
            .Include(i => i.Admission).ThenInclude(a => a.CurrentBed).ThenInclude(b => b.Ward)
            .Include(i => i.Admission).ThenInclude(a => a.AttendingPractitioner)
            .Include(i => i.Lines)
            .Include(i => i.CoverageSnapshot)
            .Include(i => i.Payments)
            .Include(i => i.StatusChanges)
            .AsSplitQuery()
            .GetRequiredAsync(id, "Invoice");
        return new InvoiceDetailsViewModel
        {
            Invoice = invoice,
            Issue = new IssueInvoiceViewModel { InvoiceId = invoice.Id, DueInDays = 30 }
        };
    }

    public async Task<int> CreateInvoiceForDischargeAsync(int admissionId)
    {
        var admission = await _db.Admissions
            .Include(a => a.CurrentBed).ThenInclude(b => b.Ward)
            .Include(a => a.Coverage).ThenInclude(c => c.Payer)
            .Include(a => a.Orders).ThenInclude(o => o.LabDetail!).ThenInclude(d => d.TestDefinition)
            .Include(a => a.Orders).ThenInclude(o => o.MedicationDetail!).ThenInclude(d => d.Drug)
            .Include(a => a.Orders).ThenInclude(o => o.Dispense)
            .AsSplitQuery()
            .GetRequiredAsync(admissionId, "Admission");

        if (admission.Status != AdmissionStatus.Discharged || admission.DischargedAt == null)
        {
            throw new InvalidStatusTransitionException("Admission", "still open", "invoice");
        }
        if (await _db.Invoices.AnyAsync(i => i.AdmissionId == admissionId))
        {
            throw new DomainValidationException($"Admission {admission.AdmissionNumber} has already been invoiced.");
        }

        var now = _clock.Now;
        var ward = admission.CurrentBed.Ward;
        var (days, roomAmount) = CalculateRoomCharge(ward.DailyRate, admission.AdmittedAt, admission.DischargedAt.Value);

        var invoice = new Invoice
        {
            InvoiceNumber = ReferenceNumbers.Next("INV", now),
            AdmissionId = admission.Id,
            PatientId = admission.PatientId,
            Status = InvoiceStatus.Draft,
            CreatedAt = now,
            RoomDays = days
        };
        invoice.Lines.Add(new InvoiceLine
        {
            LineType = InvoiceLineType.RoomCharge,
            Description = $"Room charge - {ward.Name} ({days} day{(days == 1 ? string.Empty : "s")})",
            Quantity = days,
            UnitPrice = ward.DailyRate,
            Amount = roomAmount
        });

        foreach (var order in admission.Orders.Where(o => o.Status == OrderStatus.Completed).OrderBy(o => o.CompletedAt))
        {
            if (order.Type == OrderType.Lab && order.LabDetail != null)
            {
                var test = order.LabDetail.TestDefinition;
                invoice.Lines.Add(new InvoiceLine
                {
                    LineType = InvoiceLineType.LabCharge,
                    Description = $"Lab {test.Code} {test.Name} ({order.OrderNumber})",
                    Quantity = 1,
                    UnitPrice = test.Price,
                    Amount = test.Price,
                    ClinicalOrderId = order.Id
                });
            }
            else if (order.Type == OrderType.Medication && order.MedicationDetail != null && order.Dispense != null)
            {
                var quantity = order.Dispense.Quantity;
                var unitPrice = order.Dispense.UnitPrice;
                invoice.Lines.Add(new InvoiceLine
                {
                    LineType = InvoiceLineType.MedicationCharge,
                    Description = $"{order.MedicationDetail.Drug.DisplayName} x{quantity} ({order.OrderNumber})",
                    Quantity = quantity,
                    UnitPrice = unitPrice,
                    Amount = Math.Round(unitPrice * quantity, 2, MidpointRounding.AwayFromZero),
                    ClinicalOrderId = order.Id
                });
            }
        }

        invoice.SubTotal = invoice.Lines.Sum(l => l.Amount);
        var coverage = admission.Coverage;
        invoice.CoveredAmount = Math.Round(invoice.SubTotal * coverage.CoveragePercent / 100m, 2, MidpointRounding.AwayFromZero);
        invoice.PatientResponsibility = invoice.SubTotal - invoice.CoveredAmount;
        invoice.CoverageSnapshot = new InvoiceCoverageSnapshot
        {
            InsuranceCoverageId = coverage.Id,
            PayerCode = coverage.Payer.Code,
            PayerName = coverage.Payer.Name,
            MemberId = coverage.MemberId,
            GroupNumber = coverage.GroupNumber,
            CoveragePercent = coverage.CoveragePercent,
            CoveredAmount = invoice.CoveredAmount,
            CapturedAt = now
        };
        _db.Invoices.Add(invoice);

        await _db.InTransactionAsync(async () =>
        {
            await _db.SaveChangesAsync();
            _audit.Record(AuditActions.InvoiceCreated, nameof(Invoice), invoice.Id,
                $"{invoice.InvoiceNumber} for {admission.AdmissionNumber}: subtotal {invoice.SubTotal:0.00}, covered {invoice.CoveredAmount:0.00}, status Draft");
            await _db.SaveChangesAsync();
        });
        return invoice.Id;
    }

    public async Task IssueAsync(int id, int dueInDays)
    {
        if (dueInDays is < 1 or > 180)
        {
            throw new DomainValidationException("Payment terms must be between 1 and 180 days.", nameof(IssueInvoiceViewModel.DueInDays));
        }

        var invoice = await _db.Invoices.Include(i => i.Payments).GetRequiredAsync(id, "Invoice");
        if (invoice.Status != InvoiceStatus.Draft)
        {
            throw new InvalidStatusTransitionException("Invoice", invoice.Status.ToString(), "issue");
        }

        var now = _clock.Now;
        var user = _currentUser.UserName;
        invoice.IssuedAt = now;
        invoice.DueDate = now.Date.AddDays(dueInDays);
        invoice.ChangeStatus(InvoiceStatus.Issued, now, user, $"Due {invoice.DueDate:yyyy-MM-dd}");
        _audit.Record(AuditActions.InvoiceIssued, nameof(Invoice), invoice.Id, $"{invoice.InvoiceNumber} issued, due {invoice.DueDate:yyyy-MM-dd}");

        if (invoice.PatientResponsibility <= 0)
        {
            invoice.PaidAt = now;
            invoice.ChangeStatus(InvoiceStatus.Paid, now, user, "Fully covered by insurance");
            _audit.Record(AuditActions.InvoicePaid, nameof(Invoice), invoice.Id, $"{invoice.InvoiceNumber} fully covered by insurance");
        }

        await _db.SaveChangesAsync();
    }

    public async Task<PaymentViewModel> GetPaymentFormAsync(int id)
    {
        var invoice = await _db.Invoices.AsNoTracking()
            .Include(i => i.Patient)
            .Include(i => i.Payments)
            .GetRequiredAsync(id, "Invoice");
        EnsureCanTakePayment(invoice);
        return new PaymentViewModel
        {
            InvoiceId = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            PatientName = invoice.Patient.FullName,
            BalanceDue = invoice.BalanceDue,
            Amount = invoice.BalanceDue
        };
    }

    public async Task RecordPaymentAsync(PaymentViewModel model)
    {
        var invoice = await _db.Invoices.Include(i => i.Payments).GetRequiredAsync(model.InvoiceId, "Invoice");
        EnsureCanTakePayment(invoice);

        var amount = model.Amount ?? 0m;
        if (amount <= 0)
        {
            throw new DomainValidationException("Payment amount must be greater than zero.", nameof(model.Amount));
        }
        if (amount > invoice.BalanceDue)
        {
            throw new DomainValidationException($"Payment of {amount:0.00} exceeds the balance due of {invoice.BalanceDue:0.00}.", nameof(model.Amount));
        }

        var now = _clock.Now;
        var user = _currentUser.UserName;
        invoice.Payments.Add(new Payment
        {
            Amount = amount,
            Method = model.Method ?? PaymentMethod.Cash,
            Reference = model.Reference?.Trim(),
            ReceivedAt = now,
            ReceivedBy = user
        });
        _audit.Record(AuditActions.PaymentRecorded, nameof(Invoice), invoice.Id, $"{amount:0.00} by {model.Method} on {invoice.InvoiceNumber}");

        if (invoice.BalanceDue == 0)
        {
            invoice.PaidAt = now;
            invoice.ChangeStatus(InvoiceStatus.Paid, now, user, "Paid in full");
            _audit.Record(AuditActions.InvoicePaid, nameof(Invoice), invoice.Id, $"{invoice.InvoiceNumber} paid in full");
        }

        await _db.SaveChangesAsync();
    }

    public async Task VoidAsync(int id, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainValidationException("A reason is required to void an invoice.", "Reason");
        }

        var invoice = await _db.Invoices.Include(i => i.Payments).GetRequiredAsync(id, "Invoice");
        if (invoice.Status is InvoiceStatus.Paid or InvoiceStatus.Void)
        {
            throw new InvalidStatusTransitionException("Invoice", invoice.Status.ToString(), "void");
        }
        if (invoice.Payments.Count > 0)
        {
            throw new DomainValidationException($"Invoice {invoice.InvoiceNumber} has {invoice.AmountPaid:0.00} in recorded payments and cannot be voided.");
        }

        var now = _clock.Now;
        invoice.VoidedAt = now;
        invoice.VoidReason = reason.Trim();
        invoice.ChangeStatus(InvoiceStatus.Void, now, _currentUser.UserName, reason.Trim());
        _audit.Record(AuditActions.InvoiceVoided, nameof(Invoice), invoice.Id, $"{invoice.InvoiceNumber}: {reason.Trim()}");
        await _db.SaveChangesAsync();
    }

    private static void EnsureCanTakePayment(Invoice invoice)
    {
        if (invoice.Status != InvoiceStatus.Issued)
        {
            throw new InvalidStatusTransitionException("Invoice", invoice.Status.ToString(), "take payment for");
        }
    }
}
