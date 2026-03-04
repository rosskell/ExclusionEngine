# Exclusion Engine (.NET Framework 4.8 / IIS)

This repository contains an **ASP.NET Web Forms** application targeting **.NET Framework 4.8** for IIS hosting.

## What this implements

- Client login page.
- Users can be assigned to multiple clients and select the active client during entry.
- Customer data capture fields:
  - Customer Number *(optional)*
  - First Name *(optional)*
  - Last Name *(optional)*
  - Address1 *(required)*
  - Address2 *(optional)*
  - City *(required)*
  - State *(required 2-letter)*
  - Zip *(optional)*
  - Email *(optional)*
- SQL Server storage with per-user/per-client scoping.
- Edit existing records from the Recent Entries grid.
- Recent entries are filtered to clients the current user is authorized to access.
- Password hashing (PBKDF2) for stored credentials.
- Admin-only user management page (`AdminUsers.aspx`) to add/update users, admin flags, and client access mappings.
- Forgot password flow:
  - `ForgotPassword.aspx` generates a token and sends a reset link to the user email.
  - `ResetPassword.aspx` consumes the token and updates password.
- Address standardization flow intended for **BCC Satori CASS Server** integration:
  - Uses a Mailroom Toolkit COM ZIP task call path in `SatoriCassService` (late-bound).
  - Before saving, entered and standardized addresses are compared.
  - The modal allows users to **accept standardized** or **keep original** and save.
  - If COM is unavailable/erroring, flow falls back to local normalization unless strict mode is enabled.

## Project layout

- `ExclusionEngine.sln`
- `ExclusionEngine.Web/ExclusionEngine.csproj`
- `ExclusionEngine.Web/Default.aspx` (entry form + modal confirmation)
- `ExclusionEngine.Web/Login.aspx`
- `ExclusionEngine.Web/AdminUsers.aspx`
- `ExclusionEngine.Web/ForgotPassword.aspx`
- `ExclusionEngine.Web/ResetPassword.aspx`
- `ExclusionEngine.Web/App_Code/Repository.cs` (SQL access + schema/seed)
- `ExclusionEngine.Web/App_Code/Security.cs` (password hashing/verification)
- `ExclusionEngine.Web/App_Code/SatoriCassService.cs` (CASS integration point)
- `ExclusionEngine.Web/App_Code/EmailService.cs` (SMTP reset email)

## Run in IIS / Visual Studio

1. Open `ExclusionEngine.sln` in Visual Studio.
2. Restore NuGet packages (Visual Studio restore or `nuget restore ExclusionEngine.sln`).
3. Ensure local SQL Server / LocalDB is available.
4. Update `Web.config` connection string as needed.
5. Optionally configure app settings in `Web.config`:
   - `SatoriCassEndpoint`, `SatoriCassUsername`, `SatoriCassPassword`
   - `AppBaseUrl` (used in reset links)
   - `FromEmail` (used by SMTP sender)
6. Configure SMTP in `system.net/mailSettings` (or machine config) for reset email delivery.
7. Run the web app.

Demo seed account (for local testing):
- Username: `demo`
- Password: `demo123`
- Demo account is seeded as admin to bootstrap user administration.

## BCC Satori CASS hookup

`SatoriCassService.StandardizeAddress(...)` uses the same call sequence from your WinForms sample:
- create ZIP task COM object
- set MailRoom server
- `PrepareTask`, `ClearAddress`
- set address fields
- `CheckAddress`
- inspect `ErrorCodes`
- `EndTask`

### Required config
- `SatoriCassMailRoomServer` (preferred, e.g. `10.0.2.37:5150`)
- `SatoriCassProgId` (default `MRTKTASKLib.ZIPTask`)

### Optional config
- `SatoriCassEndpoint` (legacy fallback server key)
- `SatoriCassTrace` (`true`/`false`) to emit `Trace` diagnostics
- `SatoriCassThrowOnError` (`true`/`false`) to fail requests instead of fallback

### Important: do **not** reference `Interop.MRTKTASKLib.dll` in this WebForms project
The `BadImageFormatException` you saw is a bitness/load-context problem. This app intentionally uses late-bound COM activation (`Type.GetTypeFromProgID`) so it does not require interop DLL loading in ASP.NET.

If COM activation still fails, verify:
- Mailroom Toolkit COM is registered on the web host.
- IIS/IIS Express process bitness matches the COM registration (x86 vs x64).
- For IIS Express in Visual Studio, toggle **Use 64-bit IIS Express** if needed and retry.


### If you still see `BadImageFormatException` for `Interop.MRTKTASKLib`
1. Remove any `Interop.MRTKTASKLib` reference from the Web project (References node and `.csproj`).
2. Delete `bin` and `obj` folders for `ExclusionEngine.Web`.
3. Clear Temporary ASP.NET files (`%LOCALAPPDATA%\Temp\Temporary ASP.NET Files`).
4. Restart Visual Studio and run again.
5. Keep IIS Express at 32-bit unless you are sure the COM registration is 64-bit.
