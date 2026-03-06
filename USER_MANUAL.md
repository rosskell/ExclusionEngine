# Exclusion Engine User Manual

This manual is for day-to-day users and administrators of the Exclusion Engine web application.

---

## 1) What this application does

Exclusion Engine lets you:
- Sign in with your user account.
- Create and manage customer address entries for authorized clients.
- Validate addresses through CASS and choose whether to save standardized or original values.
- Search, edit, and delete recent entries.
- (Admins) Manage users and client records.
- Request and complete password resets.

---

## 2) Accessing the site

1. Open the website URL provided by your IT team.
2. You will land on **Client Login**.
3. Enter your username and password, then click **Sign in**.

If you forget your password, click **Forgot password?** on the login page.

---

## 3) Login and session behavior

- After successful login, you are redirected to **Customer Entry** (dashboard).
- The top-right header shows who is signed in.
- Use **Log out** when done.
- If your session expires, you will be returned to login.

---

## 4) Dashboard: Customer Entry workflow

On the **Customer Entry** card, fill in:
- Client
- Customer Number
- First Name
- Last Name
- Address 1
- Address 2 (optional)
- City
- State (2-letter)
- Zip
- Email

Then click **Validate + Save**.

### What happens during save

1. The form is validated.
2. Your access to the selected client is verified.
3. CASS address standardization runs.
4. One of the following occurs:
   - **No CASS differences/errors**: entry is saved immediately.
   - **CASS differences**: a confirmation modal appears.
   - **CASS issue/error**: the modal appears with a warning and options.

### CASS confirmation modal options

When the modal appears, choose one:
- **Accept Standardized & Save**: stores CASS-standardized address values.
- **Keep Original & Save**: stores your originally entered address values.
- **Cancel**: does not save; returns you to editing.

---

## 5) Recent Entries (search, edit, delete)

In **Recent Entries (Authorized Clients Only)**:
- Search by **Last Name** and/or **Address 1**.
- Click **Search** to filter.
- Click **Clear** to reset search.

For each row:
- **Edit** loads that entry into the form for update.
- **Delete** removes that entry after confirmation.

### Editing an entry

- Click **Edit** in the grid.
- Form fields are populated.
- Make changes, then click **Validate + Save**.
- Use **Cancel Edit** to abandon edits.

---

## 6) Admin-only areas

Administrators see two buttons on dashboard:
- **User Admin**
- **Client Admin**

Non-admin users will not see these options.

### 6.1 User Admin

Use **User Admin** to:
- Create users
- Update users
- Enable/disable users
- Delete users
- Assign allowed clients for non-admin users
- Mark user as admin

#### Create user
1. Open **User Admin**.
2. Enter Username, Email, Password.
3. Set **Is Admin** as needed.
4. If non-admin, select **Allowed Clients**.
5. Click **Save User**.

#### Edit user
1. Click **Edit** on a user row.
2. Update fields as needed.
3. Optional: provide a new password to rotate credentials.
4. Click **Save User**.

#### Disable/Enable user
- Click **Disable/Enable** on the row.

#### Delete user
- Click **Delete** and confirm.

> Note: user actions may fail if constraints are violated (for example, duplicate username/email).

### 6.2 Client Admin

Use **Client Admin** to:
- Create client records
- Update client code/name
- Delete client records

#### Create client
1. Enter **Client Code** and **Client Name**.
2. Click **Save Client**.

#### Edit client
1. Click **Edit** for the client row.
2. Update fields.
3. Click **Update Client**.

#### Delete client
- Click **Delete** and confirm.
- Deletion may remove related records depending on database constraints.

---

## 7) Password reset flow

### Request reset link
1. On login page, click **Forgot password?**
2. Enter your email.
3. Click **Send Reset Link**.

You will see a generic success response whether the email exists or not.

### Complete reset
1. Open the reset link.
2. Enter a new password (minimum 8 characters).
3. Click **Reset Password**.

If token is invalid/expired, request a new link.

---

## 8) Messages you may see

- **Invalid credentials.** Username/password mismatch.
- **You are not authorized for the selected client.** Your user is not mapped to that client.
- **CASS validation reported an issue.** Review modal and choose standardized/original/cancel.
- **Token expired or invalid.** Reset link is no longer usable.

---

## 9) Best practices for users

- Always confirm the selected **Client** before saving.
- Use CASS standardized addresses unless you have a business reason not to.
- Use **Search** to avoid creating duplicate customer entries.
- Log out when leaving your workstation.

---

## 10) Admin operational checklist

- Regularly review disabled users.
- Verify non-admin users have only required client assignments.
- Keep client codes consistent and unique.
- Validate SMTP/AppBaseUrl settings so password reset emails work in production.
- If CASS errors appear frequently, contact IT to review Satori CASS service/COM registration.

---

## 11) Quick navigation map

- **Login**: `Login.aspx`
- **Dashboard/Entry**: `Default.aspx`
- **User Admin** (admin only): `AdminUsers.aspx`
- **Client Admin** (admin only): `ClientAdmin.aspx`
- **Forgot Password**: `ForgotPassword.aspx`
- **Reset Password**: `ResetPassword.aspx?token=...`

---

## 12) Support handoff notes

When reporting an issue to IT/support, include:
- Your username
- Approximate timestamp
- Page where issue occurred
- Exact error text shown on screen
- Whether issue occurs for one client or all clients
- For CASS issues: whether modal appeared and what option you selected
