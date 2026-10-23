using CarePath.Models.Entities;

namespace CarePath.Models.ViewModels;

public class DashboardViewModel
{
    public int OccupiedBeds { get; set; }
    public int AvailableBeds { get; set; }
    public int TotalBeds { get; set; }
    public int OpenAdmissions { get; set; }
    public int PendingLabOrders { get; set; }
    public int PendingMedicationOrders { get; set; }
    public int DraftInvoices { get; set; }
    public int TodaysAppointments { get; set; }
    public int LowStockItems { get; set; }
    public IReadOnlyList<Admission> RecentAdmissions { get; set; } = Array.Empty<Admission>();

    public decimal OccupancyPercent => TotalBeds == 0 ? 0 : Math.Round(OccupiedBeds * 100m / TotalBeds, 1);
}
