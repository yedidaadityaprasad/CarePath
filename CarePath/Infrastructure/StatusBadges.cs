using CarePath.Models.Entities;

namespace CarePath.Infrastructure;

/// <summary>Maps domain statuses to Bootstrap badge classes for consistent display.</summary>
public static class StatusBadges
{
    public static string For(BedStatus status) => status switch
    {
        BedStatus.Available => "bg-success",
        BedStatus.Occupied => "bg-primary",
        _ => "bg-secondary"
    };

    public static string For(AdmissionStatus status) => status == AdmissionStatus.Open ? "bg-primary" : "bg-secondary";

    public static string For(ShiftStatus status) => status == ShiftStatus.Scheduled ? "bg-info text-dark" : "bg-secondary";

    public static string For(AppointmentStatus status) => status switch
    {
        AppointmentStatus.Booked => "bg-info text-dark",
        AppointmentStatus.Completed => "bg-success",
        _ => "bg-secondary"
    };

    public static string For(OrderStatus status) => status switch
    {
        OrderStatus.Placed => "bg-warning text-dark",
        OrderStatus.InProgress => "bg-info text-dark",
        OrderStatus.Completed => "bg-success",
        _ => "bg-secondary"
    };

    public static string For(OrderPriority priority) => priority switch
    {
        OrderPriority.Stat => "bg-danger",
        OrderPriority.Urgent => "bg-warning text-dark",
        _ => "bg-light text-dark border"
    };

    public static string For(InvoiceStatus status) => status switch
    {
        InvoiceStatus.Draft => "bg-warning text-dark",
        InvoiceStatus.Issued => "bg-info text-dark",
        InvoiceStatus.Paid => "bg-success",
        _ => "bg-secondary"
    };

    public static string For(ResultFlag flag) => flag switch
    {
        ResultFlag.Normal => "bg-success",
        ResultFlag.Low or ResultFlag.High => "bg-warning text-dark",
        _ => "bg-danger"
    };

    public static string Active(bool isActive) => isActive ? "bg-success" : "bg-secondary";
}
