namespace CarePath.Models.Entities;

public enum Gender
{
    Female,
    Male,
    Other,
    Unknown
}

public enum AllergySeverity
{
    Mild,
    Moderate,
    Severe
}

public enum PractitionerType
{
    Physician,
    Nurse
}

public enum BedStatus
{
    Available,
    Occupied,
    OutOfService
}

public enum AdmissionStatus
{
    Open,
    Discharged
}

public enum ShiftStatus
{
    Scheduled,
    Cancelled
}

public enum AppointmentStatus
{
    Booked,
    Completed,
    Cancelled
}

public enum OrderType
{
    Lab,
    Medication
}

public enum OrderStatus
{
    Placed,
    InProgress,
    Completed,
    Cancelled
}

public enum OrderPriority
{
    Routine,
    Urgent,
    Stat
}

public enum ResultValueType
{
    Numeric,
    Text
}

public enum ResultFlag
{
    Normal,
    Low,
    High,
    Abnormal
}

public enum StockMovementType
{
    Opening,
    Restock,
    Dispense
}

public enum InvoiceStatus
{
    Draft,
    Issued,
    Paid,
    Void
}

public enum InvoiceLineType
{
    RoomCharge,
    LabCharge,
    MedicationCharge
}

public enum PaymentMethod
{
    Cash,
    Card,
    BankTransfer,
    Cheque
}
