# CarePath

CarePath is a hospital operations application built with **ASP.NET Core MVC on .NET 10** (`net10.0`).
It uses server-rendered Razor views, EF Core 10 on SQL Server, and ASP.NET Core Identity with roles.

It covers patients, insurance coverage and payers, practitioners, wards and beds, admissions
(transfer, discharge, notes, vitals), shifts, appointments, clinical orders, laboratory,
pharmacy stock and dispensing, billing (invoices and payments), user administration and an audit trail.

## Solution layout

| Path | Contents |
| --- | --- |
| `CarePath.sln` | Solution |
| `CarePath/` | MVC web project (Controllers, Views, Services, Data, Models) |
| `CarePath/Data/Migrations/` | Initial EF Core migration |
| `CarePath.Tests/` | xUnit service tests (SQLite in-memory, test project only) |
| `global.json` | Pins the .NET 10 SDK |
| `.config/dotnet-tools.json` | Local `dotnet-ef` 10.0.x tool |

## Prerequisites

1. **.NET 10 SDK** (10.0.1xx). Download it from <https://dotnet.microsoft.com/download/dotnet/10.0>,
   or on Windows run `winget install Microsoft.DotNet.SDK.10`.
   Check with `dotnet --list-sdks`. `global.json` selects 10.0.100 and rolls forward to any later 10.0 feature band.
2. **SQL Server**: LocalDB (installed with Visual Studio or SQL Server Express), or any SQL Server instance.

## Configure the connection string

The connection string is named `CarePath`:

- `CarePath/appsettings.json` defaults to LocalDB with database `CarePath`:
  `Server=(localdb)\mssqllocaldb;Database=CarePath;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True`
- `CarePath/appsettings.Development.json` overrides it with database `CarePath_Dev` for local development.

These are the only connection strings in the repository. They use Windows integrated authentication, so they contain no password.
To use a different server, override the string without editing committed files:

```bash
dotnet user-secrets set "ConnectionStrings:CarePath" "Server=...;Database=CarePath_Dev;..." --project CarePath
```

or set the environment variable `ConnectionStrings__CarePath`.

## Seed administrator

On startup the app seeds the roles **Admin, Physician, Nurse, BillingClerk, Pharmacist**, plus one Admin user from the
`SeedAdmin` configuration section. The email defaults to `admin@carepath.local`. **No password is committed.** Set one with user-secrets
(minimum 8 characters, with upper case, lower case, a digit and a symbol):

```bash
dotnet user-secrets set "SeedAdmin:Password" "<choose-a-strong-password>" --project CarePath
```

If the password is not set, the app still starts, logs a warning and skips creating the admin user.
Sign in as the admin and create the other staff accounts under **Users**.

## Create the database

```bash
dotnet tool restore
dotnet ef database update --project CarePath
```

This applies the `InitialCreate` migration, including reference data such as departments, specialties,
specimen types and drug categories.

## Run

```bash
dotnet run --project CarePath
```

Open the URL printed in the console and sign in.

## Build and test

```bash
dotnet build
dotnet test
```

The service tests use SQLite in-memory databases, so they do not need SQL Server.

## Roles and permissions

All pages require sign-in except **Login** and **Access denied**. Permissions are enforced with `[Authorize(Roles = ...)]`
on controllers and actions. The navigation menu is filtered by role, but it is not the access control.

| Role | Can |
| --- | --- |
| Admin | Maintain wards, beds, payers, drugs, lab tests, practitioners and users; set up ward stock lines; view invoices and the audit trail |
| Physician, Nurse | Manage patients, coverage, admissions, shifts, appointments and clinical orders; record lab specimens and results; view wards, beds, practitioners, drugs and stock |
| Pharmacist | Dispense medication orders; restock and set up ward stock |
| BillingClerk | Issue, take payments on, and void invoices; maintain patient coverage |

## Business rules (selected)

- Admission requires an active patient, active coverage, an available bed in an open ward, and an attending physician.
- Discharge creates a draft invoice. Placed orders are cancelled, and in-progress orders block discharge.
- Room charge is calendar days between admission and discharge, with a minimum of 1 day, at the daily rate of the ward at discharge.
  Lab and dispensed medication lines are added at their recorded prices. The coverage percentage splits the total between payer and patient.
- Records are never hard-deleted. Status actions (deactivate, cancel, close, void) replace delete. Every change writes an audit entry.
- Domain rule violations raise typed domain exceptions. They are shown as form errors, or by a global filter that renders a friendly error page and logs the details.
