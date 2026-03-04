# Exclusion Engine (.NET Framework 4.8 / IIS)

This repository contains an **ASP.NET Web Forms** application targeting **.NET Framework 4.8** for IIS hosting.

## What this implements

- Client login page.
- Users can be assigned to multiple clients and select the active client during entry.
- Customer data capture fields:
  - Customer Number
  - First Name
  - Last Name
  - Address1
  - Address2
  - City
  - State
  - Zip
  - Email
- SQL Server storage with per-user/per-client scoping.
- Edit existing records from the Recent Entries grid.
- Recent entries are filtered to clients the current user is authorized to access.
- Password hashing (PBKDF2) for stored credentials.
- Address standardization flow intended for **BCC Satori CASS Server** integration:
  - Current implementation contains a placeholder in `SatoriCassService`.
  - Before saving, entered and standardized addresses are compared.
  - The modal allows users to **accept standardized** or **keep original** and save.

## Project layout

- `ExclusionEngine.sln`
- `ExclusionEngine.Web/ExclusionEngine.csproj`
- `ExclusionEngine.Web/Default.aspx` (entry form + modal confirmation)
- `ExclusionEngine.Web/Login.aspx`
- `ExclusionEngine.Web/App_Code/Repository.cs` (SQL access + schema/seed)
- `ExclusionEngine.Web/App_Code/Security.cs` (password hashing/verification)
- `ExclusionEngine.Web/App_Code/SatoriCassService.cs` (CASS integration point)

## Run in IIS / Visual Studio

1. Open `ExclusionEngine.sln` in Visual Studio.
2. Restore NuGet packages (Visual Studio restore or `nuget restore ExclusionEngine.sln`).
3. Ensure local SQL Server / LocalDB is available.
4. Update `Web.config` connection string as needed.
5. Optionally configure `SatoriCassEndpoint`, `SatoriCassUsername`, and `SatoriCassPassword`.
6. Run the web app.

Demo seed account (for local testing):
- Username: `demo`
- Password: `demo123`

## BCC Satori CASS hookup

Replace the logic inside `SatoriCassService.StandardizeAddress(...)` with your actual BCC Satori server call (SOAP/REST based on your deployment), then map the standardized response fields back into `CustomerEntryInput`.
