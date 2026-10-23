namespace CarePath.Services;

public static class AuditActions
{
    public const string Register = "Register";
    public const string Deactivate = "Deactivate";
    public const string Reactivate = "Reactivate";
    public const string AttachCoverage = "AttachCoverage";
    public const string TerminateCoverage = "TerminateCoverage";
    public const string CloseWard = "CloseWard";
    public const string ReopenWard = "ReopenWard";
    public const string BedOutOfService = "BedOutOfService";
    public const string BedReturnedToService = "BedReturnedToService";
    public const string Admit = "Admit";
    public const string Transfer = "Transfer";
    public const string Discharge = "Discharge";
    public const string ScheduleShift = "ScheduleShift";
    public const string CancelShift = "CancelShift";
    public const string PlaceOrder = "PlaceOrder";
    public const string CancelOrder = "CancelOrder";
    public const string ResultEntered = "ResultEntered";
    public const string Dispense = "Dispense";
    public const string Restock = "Restock";
    public const string InvoiceCreated = "InvoiceCreated";
    public const string InvoiceIssued = "InvoiceIssued";
    public const string PaymentRecorded = "PaymentRecorded";
    public const string InvoicePaid = "InvoicePaid";
    public const string InvoiceVoided = "InvoiceVoided";
    public const string UserCreated = "UserCreated";
    public const string UserUpdated = "UserUpdated";
}
