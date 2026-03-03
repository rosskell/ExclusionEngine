# Exclusion Engine (.NET Framework 4.8 / IIS)

This repository now contains an **ASP.NET Web Forms** application targeting **.NET Framework 4.8** for IIS hosting.

## What this implements

- Client login page.
- User can be assigned to multiple clients and select the active client at data entry time.
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
- SQL Server storage with user/client association.
- Address standardization flow intended for **BCC Satori CASS Server** integration:
  - Current implementation includes a `SatoriCassService` placeholder.
  - Before saving, the app compares entered vs standardized address.
  - A popup/modal asks the user to accept the standardized address change.

## Project layout

- `ExclusionEngine.Web/ExclusionEngine.sln`
- `ExclusionEngine.Web/ExclusionEngine.Web.csproj`
- `ExclusionEngine.Web/Default.aspx` (entry form + modal confirmation)
- `ExclusionEngine.Web/Login.aspx`
- `ExclusionEngine.Web/App_Code/Repository.cs` (SQL access + schema/seed)
- `ExclusionEngine.Web/App_Code/SatoriCassService.cs` (CASS integration point)

## Run in IIS / Visual Studio

1. Open `ExclusionEngine.Web/ExclusionEngine.sln` in Visual Studio.
2. Ensure local SQL Server / LocalDB is available.
3. Update `Web.config` connection string as needed.
4. Run the web app.

Demo seed account (for local testing):
- Username: `demo`
- Password: `demo123`

## BCC Satori CASS hookup

Replace the logic inside `SatoriCassService.StandardizeAddress(...)` with your actual BCC Satori server call (SOAP/REST based on your deployment), then map the standardized response fields back into `CustomerEntryInput`.
