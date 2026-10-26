namespace CarePath.Infrastructure;

/// <summary>
/// Role name constants used throughout the application for authorization.
/// .NET 10: Role-based authorization via [Authorize(Roles = ...)] is unchanged.
/// </summary>
public static class Roles
{
    public const string Admin = "Admin";
    public const string Physician = "Physician";
    public const string Nurse = "Nurse";
    public const string BillingClerk = "BillingClerk";
    public const string Pharmacist = "Pharmacist";

    // Composite role strings used with [Authorize(Roles = "...")] — comma-separated means OR
    public const string Clinical = Physician + "," + Nurse;
    public const string ClinicalAndAdmin = Admin + "," + Physician + "," + Nurse;
    public const string PharmacyAndAdmin = Admin + "," + Pharmacist;
    public const string PharmacyViewers = Admin + "," + Pharmacist + "," + Nurse + "," + Physician;
    public const string BillingViewers = Admin + "," + BillingClerk;
    public const string CoverageEditors = Physician + "," + Nurse + "," + BillingClerk;
    public const string CoverageViewers = Admin + "," + Physician + "," + Nurse + "," + BillingClerk;

    /// <summary>All individual role names, used for seeding and user-management dropdowns.</summary>
    public static readonly IReadOnlyList<string> All = [Admin, Physician, Nurse, BillingClerk, Pharmacist];
}
