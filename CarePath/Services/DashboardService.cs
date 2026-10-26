using CarePath.Data;
using CarePath.Models.Entities;
using CarePath.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CarePath.Services;

/// <summary>
/// Aggregates key operational metrics for the home dashboard.
/// .NET 10: All database queries use CancellationToken for proper request cancellation.
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _db;
    private readonly IClock _clock;

    public DashboardService(ApplicationDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<DashboardViewModel> GetAsync(CancellationToken cancellationToken = default)
    {
        var today = _clock.Now.Date;
        var tomorrow = today.AddDays(1);

        var bedCounts = await _db.Beds.AsNoTracking()
            .GroupBy(b => b.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return new DashboardViewModel
        {
            OccupiedBeds = bedCounts.Where(c => c.Status == BedStatus.Occupied).Sum(c => c.Count),
            AvailableBeds = await _db.Beds.CountAsync(
                b => b.Status == BedStatus.Available && !b.Ward.IsClosed, cancellationToken),
            TotalBeds = bedCounts.Sum(c => c.Count),
            OpenAdmissions = await _db.Admissions.CountAsync(
                a => a.Status == AdmissionStatus.Open, cancellationToken),
            PendingLabOrders = await _db.ClinicalOrders.CountAsync(
                o => o.Type == OrderType.Lab
                     && (o.Status == OrderStatus.Placed || o.Status == OrderStatus.InProgress),
                cancellationToken),
            PendingMedicationOrders = await _db.ClinicalOrders.CountAsync(
                o => o.Type == OrderType.Medication
                     && (o.Status == OrderStatus.Placed || o.Status == OrderStatus.InProgress),
                cancellationToken),
            DraftInvoices = await _db.Invoices.CountAsync(
                i => i.Status == InvoiceStatus.Draft, cancellationToken),
            TodaysAppointments = await _db.Appointments.CountAsync(
                a => a.Status != AppointmentStatus.Cancelled
                     && a.StartsAt >= today && a.StartsAt < tomorrow,
                cancellationToken),
            LowStockItems = await _db.WardStocks.CountAsync(
                s => s.QuantityOnHand <= s.ReorderLevel, cancellationToken),
            RecentAdmissions = await _db.Admissions.AsNoTracking()
                .Include(a => a.Patient)
                .Include(a => a.CurrentBed).ThenInclude(b => b.Ward)
                .Where(a => a.Status == AdmissionStatus.Open)
                .OrderByDescending(a => a.AdmittedAt)
                .Take(8)
                .ToListAsync(cancellationToken)
        };
    }
}
