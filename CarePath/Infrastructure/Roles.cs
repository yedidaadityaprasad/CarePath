namespace CarePath.Infrastructure;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Physician = "Physician";
    public const string Nurse = "Nurse";
    public const string BillingClerk = "BillingClerk";
    public const string Pharmacist = "Pharmacist";

    public const string Clinical = Physician + "," + Nurse;
    public const string ClinicalAndAdmin = Admin + "," + Physician + "," + Nurse;
    public const string PharmacyAndAdmin = Admin + "," + Pharmacist;
    public const string PharmacyViewers = Admin + "," + Pharmacist + "," + Nurse + "," + Physician;
    public const string BillingViewers = Admin + "," + BillingClerk;
    public const string CoverageEditors = Physician + "," + Nurse + "," + BillingClerk;
    public const string CoverageViewers = Admin + "," + Physician + "," + Nurse + "," + BillingClerk;

    public static readonly IReadOnlyList<string> All = new[] { Admin, Physician, Nurse, BillingClerk, Pharmacist };
}
