using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ExclusionEngine.Web
{
    public static class Repository
    {
        private static string ConnectionString => ConfigurationManager.ConnectionStrings["ExclusionDb"].ConnectionString;

        public static void EnsureSchema()
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                var sql = @"
IF OBJECT_ID('dbo.Users','U') IS NULL
CREATE TABLE dbo.Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(100) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(200) NOT NULL,
    Email NVARCHAR(200) NULL,
    IsAdmin BIT NOT NULL DEFAULT(0),
    IsDisabled BIT NOT NULL DEFAULT(0),
    PasswordResetToken NVARCHAR(200) NULL,
    PasswordResetExpiresUtc DATETIME2 NULL
);
IF OBJECT_ID('dbo.Clients','U') IS NULL
CREATE TABLE dbo.Clients (
    ClientId INT IDENTITY(1,1) PRIMARY KEY,
    ClientCode NVARCHAR(50) NOT NULL UNIQUE,
    ClientName NVARCHAR(200) NOT NULL,
    IsActive BIT NOT NULL DEFAULT(1)
);
IF OBJECT_ID('dbo.UserClients','U') IS NULL
CREATE TABLE dbo.UserClients (
    UserId INT NOT NULL,
    ClientId INT NOT NULL,
    CONSTRAINT PK_UserClients PRIMARY KEY (UserId, ClientId),
    CONSTRAINT FK_UserClients_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId),
    CONSTRAINT FK_UserClients_Clients FOREIGN KEY (ClientId) REFERENCES dbo.Clients(ClientId)
);
IF OBJECT_ID('dbo.CustomerEntries','U') IS NULL
CREATE TABLE dbo.CustomerEntries (
    EntryId INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    ClientId INT NOT NULL,
    CustomerNumber NVARCHAR(50) NOT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Address1 NVARCHAR(200) NOT NULL,
    Address2 NVARCHAR(200) NULL,
    City NVARCHAR(100) NOT NULL,
    State NVARCHAR(2) NOT NULL,
    Zip NVARCHAR(10) NOT NULL,
    Zip4 NVARCHAR(4) NULL,
    DeliveryPointBarcode NVARCHAR(32) NULL,
    Email NVARCHAR(200) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_CustomerEntries_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId),
    CONSTRAINT FK_CustomerEntries_Clients FOREIGN KEY (ClientId) REFERENCES dbo.Clients(ClientId)
);

IF COL_LENGTH('dbo.Users', 'Email') IS NULL
    ALTER TABLE dbo.Users ADD Email NVARCHAR(200) NULL;
IF COL_LENGTH('dbo.Users', 'IsAdmin') IS NULL
    ALTER TABLE dbo.Users ADD IsAdmin BIT NOT NULL CONSTRAINT DF_Users_IsAdmin DEFAULT(0);
IF COL_LENGTH('dbo.Users', 'IsDisabled') IS NULL
    ALTER TABLE dbo.Users ADD IsDisabled BIT NOT NULL CONSTRAINT DF_Users_IsDisabled DEFAULT(0);
IF COL_LENGTH('dbo.Users', 'PasswordResetToken') IS NULL
    ALTER TABLE dbo.Users ADD PasswordResetToken NVARCHAR(200) NULL;
IF COL_LENGTH('dbo.Users', 'PasswordResetExpiresUtc') IS NULL
    ALTER TABLE dbo.Users ADD PasswordResetExpiresUtc DATETIME2 NULL;

IF COL_LENGTH('dbo.CustomerEntries', 'Zip4') IS NULL
    ALTER TABLE dbo.CustomerEntries ADD Zip4 NVARCHAR(4) NULL;
IF COL_LENGTH('dbo.CustomerEntries', 'DeliveryPointBarcode') IS NULL
    ALTER TABLE dbo.CustomerEntries ADD DeliveryPointBarcode NVARCHAR(32) NULL;
IF COL_LENGTH('dbo.Clients', 'IsActive') IS NULL
    ALTER TABLE dbo.Clients ADD IsActive BIT NOT NULL CONSTRAINT DF_Clients_IsActive DEFAULT(1);
";
                new SqlCommand(sql, conn).ExecuteNonQuery();
            }
        }

        public static void EnsureSeedData()
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                var cmd = new SqlCommand(@"
IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE Username = 'demo')
BEGIN
    INSERT INTO dbo.Users (Username, PasswordHash, Email, IsAdmin, IsDisabled) VALUES ('demo', @demoPasswordHash, 'demo@example.com', 1, 0);
END
ELSE
BEGIN
    UPDATE dbo.Users 
    SET Email = COALESCE(NULLIF(Email,''), 'demo@example.com'),
        IsAdmin = 1,
        IsDisabled = 0
    WHERE Username = 'demo';
END
IF EXISTS (SELECT 1 FROM dbo.Clients WHERE ClientCode = 'CLI-ALPHA')
BEGIN
    UPDATE dbo.Clients SET IsActive = 1 WHERE ClientCode = 'CLI-ALPHA';
END
IF NOT EXISTS (SELECT 1 FROM dbo.Clients WHERE ClientCode = 'CLI-ALPHA')
BEGIN
    INSERT INTO dbo.Clients (ClientCode, ClientName, IsActive) VALUES ('CLI-ALPHA', 'Alpha Logistics', 1);
END
IF EXISTS (SELECT 1 FROM dbo.Clients WHERE ClientCode = 'CLI-BETA')
BEGIN
    UPDATE dbo.Clients SET IsActive = 1 WHERE ClientCode = 'CLI-BETA';
END
IF NOT EXISTS (SELECT 1 FROM dbo.Clients WHERE ClientCode = 'CLI-BETA')
BEGIN
    INSERT INTO dbo.Clients (ClientCode, ClientName, IsActive) VALUES ('CLI-BETA', 'Beta Retail', 1);
END", conn);
                cmd.Parameters.AddWithValue("@demoPasswordHash", Security.HashPassword("demo123"));
                cmd.ExecuteNonQuery();
            }
        }

        public static bool IsAdminUser(int userId)
        {
            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand("SELECT CASE WHEN IsAdmin = 1 THEN 1 ELSE 0 END FROM dbo.Users WHERE UserId=@u", conn))
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@u", userId);
                return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
            }
        }

        public static UserModel ValidateUser(string username, string password)
        {
            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand("SELECT UserId, Username, PasswordHash, COALESCE(Email,''), IsAdmin, IsDisabled FROM dbo.Users WHERE Username = @u", conn))
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@u", username ?? string.Empty);
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read()) return null;
                    var hash = reader.GetString(2);
                    var disabled = reader.GetBoolean(5);
                    if (disabled) return null;
                    if (!Security.VerifyPassword(password, hash)) return null;

                    return new UserModel
                    {
                        UserId = reader.GetInt32(0),
                        Username = reader.GetString(1),
                        Email = reader.GetString(3),
                        IsAdmin = reader.GetBoolean(4),
                        IsDisabled = disabled
                    };
                }
            }
        }

        public static List<ClientModel> GetAllClients(bool includeInactive = false)
        {
            var result = new List<ClientModel>();
            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand("SELECT ClientId, ClientCode, ClientName, IsActive FROM dbo.Clients WHERE @includeInactive = 1 OR IsActive = 1 ORDER BY ClientName", conn))
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@includeInactive", includeInactive ? 1 : 0);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new ClientModel
                        {
                            ClientId = reader.GetInt32(0),
                            ClientCode = reader.GetString(1),
                            ClientName = reader.GetString(2),
                            IsActive = reader.GetBoolean(3)
                        });
                    }
                }
            }

            return result;
        }

        public static ClientModel GetClientById(int clientId)
        {
            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand("SELECT ClientId, ClientCode, ClientName, IsActive FROM dbo.Clients WHERE ClientId=@id", conn))
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@id", clientId);
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read()) return null;
                    return new ClientModel
                    {
                        ClientId = reader.GetInt32(0),
                        ClientCode = reader.GetString(1),
                        ClientName = reader.GetString(2),
                        IsActive = reader.GetBoolean(3)
                    };
                }
            }
        }

        public static int CreateClient(ClientModel client)
        {
            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(@"
INSERT INTO dbo.Clients(ClientCode, ClientName, IsActive)
VALUES(@code, @name, @active);
SELECT CAST(SCOPE_IDENTITY() AS INT);", conn))
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@code", client.ClientCode ?? string.Empty);
                cmd.Parameters.AddWithValue("@name", client.ClientName ?? string.Empty);
                cmd.Parameters.AddWithValue("@active", client.IsActive ? 1 : 0);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public static void UpdateClient(ClientModel client)
        {
            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand("UPDATE dbo.Clients SET ClientCode=@code, ClientName=@name, IsActive=@active WHERE ClientId=@id", conn))
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@id", client.ClientId);
                cmd.Parameters.AddWithValue("@code", client.ClientCode ?? string.Empty);
                cmd.Parameters.AddWithValue("@name", client.ClientName ?? string.Empty);
                cmd.Parameters.AddWithValue("@active", client.IsActive ? 1 : 0);
                cmd.ExecuteNonQuery();
            }
        }

        public static void DeleteClient(int clientId)
        {
            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand("UPDATE dbo.Clients SET IsActive = 0 WHERE ClientId=@id", conn))
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@id", clientId);
                cmd.ExecuteNonQuery();
            }
        }

        public static List<ClientModel> GetClientsForUser(int userId)
        {
            if (IsAdminUser(userId))
            {
                return GetAllClients();
            }

            var result = new List<ClientModel>();
            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(@"
SELECT c.ClientId, c.ClientCode, c.ClientName, c.IsActive
FROM dbo.Clients c
INNER JOIN dbo.UserClients uc ON c.ClientId = uc.ClientId
WHERE uc.UserId = @u
  AND c.IsActive = 1
ORDER BY c.ClientName", conn))
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@u", userId);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        result.Add(new ClientModel
                        {
                            ClientId = reader.GetInt32(0),
                            ClientCode = reader.GetString(1),
                            ClientName = reader.GetString(2),
                            IsActive = reader.GetBoolean(3)
                        });
                    }
                }
            }

            return result;
        }

        public static bool UserHasClientAccess(int userId, int clientId)
        {
            if (IsAdminUser(userId)) return true;

            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(@"
SELECT CASE WHEN EXISTS (
    SELECT 1 FROM dbo.UserClients WHERE UserId = @u AND ClientId = @c
) THEN 1 ELSE 0 END", conn))
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@u", userId);
                cmd.Parameters.AddWithValue("@c", clientId);
                return Convert.ToInt32(cmd.ExecuteScalar()) == 1;
            }
        }

        public static void SaveEntry(int userId, CustomerEntryInput input)
        {
            if (!UserHasClientAccess(userId, input.ClientId))
            {
                throw new InvalidOperationException("User is not authorized for this client.");
            }

            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(@"
INSERT INTO dbo.CustomerEntries
(UserId, ClientId, CustomerNumber, FirstName, LastName, Address1, Address2, City, State, Zip, Zip4, DeliveryPointBarcode, Email)
VALUES
(@u, @c, @n, @fn, @ln, @a1, @a2, @city, @st, @zip, @zip4, @dpb, @email)", conn))
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@u", userId);
                cmd.Parameters.AddWithValue("@c", input.ClientId);
                cmd.Parameters.AddWithValue("@n", input.CustomerNumber ?? string.Empty);
                cmd.Parameters.AddWithValue("@fn", input.FirstName ?? string.Empty);
                cmd.Parameters.AddWithValue("@ln", input.LastName ?? string.Empty);
                cmd.Parameters.AddWithValue("@a1", input.Address1 ?? string.Empty);
                cmd.Parameters.AddWithValue("@a2", string.IsNullOrWhiteSpace(input.Address2) ? (object)DBNull.Value : input.Address2);
                cmd.Parameters.AddWithValue("@city", input.City ?? string.Empty);
                cmd.Parameters.AddWithValue("@st", input.State ?? string.Empty);
                cmd.Parameters.AddWithValue("@zip", input.Zip ?? string.Empty);
                cmd.Parameters.AddWithValue("@zip4", string.IsNullOrWhiteSpace(input.Zip4) ? (object)DBNull.Value : input.Zip4);
                cmd.Parameters.AddWithValue("@dpb", string.IsNullOrWhiteSpace(input.DeliveryPointBarcode) ? (object)DBNull.Value : input.DeliveryPointBarcode);
                cmd.Parameters.AddWithValue("@email", string.IsNullOrWhiteSpace(input.Email) ? (object)DBNull.Value : input.Email);
                cmd.ExecuteNonQuery();
            }
        }

        public static CustomerEntryInput GetEntryForEdit(int userId, int entryId)
        {
            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(@"
SELECT ce.EntryId, ce.ClientId, ce.CustomerNumber, ce.FirstName, ce.LastName,
       ce.Address1, ce.Address2, ce.City, ce.State, ce.Zip, ce.Zip4, ce.DeliveryPointBarcode, ce.Email
FROM dbo.CustomerEntries ce
INNER JOIN dbo.Clients c ON c.ClientId = ce.ClientId
WHERE ce.EntryId = @entryId", conn))
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@entryId", entryId);

                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read()) return null;

                    var clientId = reader.GetInt32(1);
                    if (!UserHasClientAccess(userId, clientId)) return null;

                    return new CustomerEntryInput
                    {
                        EntryId = reader.GetInt32(0),
                        ClientId = clientId,
                        CustomerNumber = reader.GetString(2),
                        FirstName = reader.GetString(3),
                        LastName = reader.GetString(4),
                        Address1 = reader.GetString(5),
                        Address2 = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                        City = reader.GetString(7),
                        State = reader.GetString(8),
                        Zip = reader.GetString(9),
                        Zip4 = reader.IsDBNull(10) ? string.Empty : reader.GetString(10),
                        DeliveryPointBarcode = reader.IsDBNull(11) ? string.Empty : reader.GetString(11),
                        Email = reader.IsDBNull(12) ? string.Empty : reader.GetString(12)
                    };
                }
            }
        }

        public static void UpdateEntry(int userId, int entryId, CustomerEntryInput input)
        {
            if (!UserHasClientAccess(userId, input.ClientId))
            {
                throw new InvalidOperationException("User is not authorized for this client.");
            }

            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(@"
UPDATE dbo.CustomerEntries
SET ClientId = @c,
    CustomerNumber = @n,
    FirstName = @fn,
    LastName = @ln,
    Address1 = @a1,
    Address2 = @a2,
    City = @city,
    State = @st,
    Zip = @zip,
    Zip4 = @zip4,
    DeliveryPointBarcode = @dpb,
    Email = @email
WHERE EntryId = @entryId", conn))
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@entryId", entryId);
                cmd.Parameters.AddWithValue("@c", input.ClientId);
                cmd.Parameters.AddWithValue("@n", input.CustomerNumber ?? string.Empty);
                cmd.Parameters.AddWithValue("@fn", input.FirstName ?? string.Empty);
                cmd.Parameters.AddWithValue("@ln", input.LastName ?? string.Empty);
                cmd.Parameters.AddWithValue("@a1", input.Address1 ?? string.Empty);
                cmd.Parameters.AddWithValue("@a2", string.IsNullOrWhiteSpace(input.Address2) ? (object)DBNull.Value : input.Address2);
                cmd.Parameters.AddWithValue("@city", input.City ?? string.Empty);
                cmd.Parameters.AddWithValue("@st", input.State ?? string.Empty);
                cmd.Parameters.AddWithValue("@zip", input.Zip ?? string.Empty);
                cmd.Parameters.AddWithValue("@zip4", string.IsNullOrWhiteSpace(input.Zip4) ? (object)DBNull.Value : input.Zip4);
                cmd.Parameters.AddWithValue("@dpb", string.IsNullOrWhiteSpace(input.DeliveryPointBarcode) ? (object)DBNull.Value : input.DeliveryPointBarcode);
                cmd.Parameters.AddWithValue("@email", string.IsNullOrWhiteSpace(input.Email) ? (object)DBNull.Value : input.Email);

                var affected = cmd.ExecuteNonQuery();
                if (affected == 0)
                {
                    throw new InvalidOperationException("Entry not found.");
                }
            }
        }

        public static void DeleteEntry(int userId, int entryId)
        {
            var existing = GetEntryForEdit(userId, entryId);
            if (existing == null)
            {
                throw new InvalidOperationException("Entry not found or not authorized.");
            }

            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand("DELETE FROM dbo.CustomerEntries WHERE EntryId=@entryId", conn))
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@entryId", entryId);
                cmd.ExecuteNonQuery();
            }
        }

        public static DataTable GetRecentEntriesForUser(int userId, string lastNameContains = null, string address1Contains = null)
        {
            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(@"
SELECT TOP 20
    ce.EntryId,
    c.ClientCode + ' - ' + c.ClientName AS ClientName,
    ce.CustomerNumber,
    ce.FirstName + ' ' + ce.LastName AS FullName,
    ce.Address1 + COALESCE(', ' + NULLIF(ce.Address2,''), '') + ', ' + ce.City + ', ' + ce.State + ' ' + ce.Zip + COALESCE('-' + NULLIF(ce.Zip4,''), '') AS FormattedAddress,
    COALESCE(ce.Zip4, '') AS Zip4,
    COALESCE(ce.DeliveryPointBarcode, '') AS DeliveryPointBarcode,
    ce.Email,
    ce.CreatedAt
FROM dbo.CustomerEntries ce
INNER JOIN dbo.Clients c ON ce.ClientId = c.ClientId
WHERE (
    ce.ClientId IN (
        SELECT ClientId FROM dbo.UserClients WHERE UserId = @u
    )
    OR @isAdmin = 1
)
AND (@lastName = '' OR ce.LastName LIKE '%' + @lastName + '%')
AND (@address1 = '' OR ce.Address1 LIKE '%' + @address1 + '%')
ORDER BY ce.CreatedAt DESC", conn))
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@u", userId);
                cmd.Parameters.AddWithValue("@isAdmin", IsAdminUser(userId));
                cmd.Parameters.AddWithValue("@lastName", lastNameContains ?? string.Empty);
                cmd.Parameters.AddWithValue("@address1", address1Contains ?? string.Empty);
                var dt = new DataTable();
                dt.Load(cmd.ExecuteReader());
                return dt;
            }
        }

        public static DataTable GetCustomerDataForUser(int userId, int? clientId = null, string lastNameContains = null, string address1Contains = null)
        {
            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(@"
SELECT
    ce.EntryId,
    ce.ClientId,
    c.ClientCode + ' - ' + c.ClientName AS ClientName,
    ce.CustomerNumber,
    ce.FirstName,
    ce.LastName,
    ce.Address1,
    COALESCE(ce.Address2, '') AS Address2,
    ce.City,
    ce.State,
    ce.Zip,
    COALESCE(ce.Zip4, '') AS Zip4,
    COALESCE(ce.DeliveryPointBarcode, '') AS DeliveryPointBarcode,
    COALESCE(ce.Email, '') AS Email,
    ce.CreatedAt
FROM dbo.CustomerEntries ce
INNER JOIN dbo.Clients c ON ce.ClientId = c.ClientId
WHERE (
    ce.ClientId IN (
        SELECT ClientId FROM dbo.UserClients WHERE UserId = @u
    )
    OR @isAdmin = 1
)
AND (@clientId = 0 OR ce.ClientId = @clientId)
AND (@lastName = '' OR ce.LastName LIKE '%' + @lastName + '%')
AND (@address1 = '' OR ce.Address1 LIKE '%' + @address1 + '%')
ORDER BY ce.CreatedAt DESC", conn))
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@u", userId);
                cmd.Parameters.AddWithValue("@isAdmin", IsAdminUser(userId));
                cmd.Parameters.AddWithValue("@clientId", clientId.GetValueOrDefault());
                cmd.Parameters.AddWithValue("@lastName", lastNameContains ?? string.Empty);
                cmd.Parameters.AddWithValue("@address1", address1Contains ?? string.Empty);
                var dt = new DataTable();
                dt.Load(cmd.ExecuteReader());
                return dt;
            }
        }

        public static DataTable GetAllUsersForAdmin()
        {
            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(@"
SELECT 
    u.UserId,
    u.Username,
    COALESCE(u.Email, '') AS Email,
    u.IsAdmin,
    u.IsDisabled,
    CASE WHEN u.IsAdmin = 1 THEN '[ALL CLIENTS]'
         ELSE STUFF((
            SELECT ', ' + c.ClientCode
            FROM dbo.UserClients uc2
            INNER JOIN dbo.Clients c ON c.ClientId = uc2.ClientId
            WHERE uc2.UserId = u.UserId
            ORDER BY c.ClientCode
            FOR XML PATH(''), TYPE
         ).value('.', 'nvarchar(max)'), 1, 2, '')
    END AS ClientCodes
FROM dbo.Users u
ORDER BY u.Username", conn))
            {
                conn.Open();
                var dt = new DataTable();
                dt.Load(cmd.ExecuteReader());
                return dt;
            }
        }

        public static UserAdminModel GetUserForAdmin(int userId)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                UserAdminModel model = null;

                using (var cmd = new SqlCommand("SELECT UserId, Username, COALESCE(Email,''), IsAdmin, IsDisabled FROM dbo.Users WHERE UserId=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read()) return null;
                        model = new UserAdminModel
                        {
                            UserId = reader.GetInt32(0),
                            Username = reader.GetString(1),
                            Email = reader.GetString(2),
                            IsAdmin = reader.GetBoolean(3),
                            IsDisabled = reader.GetBoolean(4)
                        };
                    }
                }

                using (var cmd = new SqlCommand("SELECT ClientId FROM dbo.UserClients WHERE UserId=@id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", userId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            model.ClientIds.Add(reader.GetInt32(0));
                        }
                    }
                }

                return model;
            }
        }

        public static int CreateUser(UserAdminModel user, string password)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    int userId;
                    using (var cmd = new SqlCommand(@"
INSERT INTO dbo.Users(Username, PasswordHash, Email, IsAdmin, IsDisabled)
VALUES(@username, @passwordHash, @email, @isAdmin, @isDisabled);
SELECT CAST(SCOPE_IDENTITY() AS INT);", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@username", user.Username ?? string.Empty);
                        cmd.Parameters.AddWithValue("@passwordHash", Security.HashPassword(password ?? string.Empty));
                        cmd.Parameters.AddWithValue("@email", string.IsNullOrWhiteSpace(user.Email) ? (object)DBNull.Value : user.Email.Trim());
                        cmd.Parameters.AddWithValue("@isAdmin", user.IsAdmin);
                        cmd.Parameters.AddWithValue("@isDisabled", user.IsDisabled);
                        userId = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    ReplaceUserClientMappings(conn, tx, userId, user.IsAdmin ? new List<int>() : user.ClientIds);
                    tx.Commit();
                    return userId;
                }
            }
        }

        public static void UpdateUser(UserAdminModel user, string newPassword)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                    var sql = @"
UPDATE dbo.Users
SET Username = @username,
    Email = @email,
    IsAdmin = @isAdmin,
    IsDisabled = @isDisabled";

                    if (!string.IsNullOrWhiteSpace(newPassword))
                    {
                        sql += ", PasswordHash = @passwordHash";
                    }

                    sql += " WHERE UserId = @userId";

                    using (var cmd = new SqlCommand(sql, conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@userId", user.UserId);
                        cmd.Parameters.AddWithValue("@username", user.Username ?? string.Empty);
                        cmd.Parameters.AddWithValue("@email", string.IsNullOrWhiteSpace(user.Email) ? (object)DBNull.Value : user.Email.Trim());
                        cmd.Parameters.AddWithValue("@isAdmin", user.IsAdmin);
                        cmd.Parameters.AddWithValue("@isDisabled", user.IsDisabled);
                        if (!string.IsNullOrWhiteSpace(newPassword))
                        {
                            cmd.Parameters.AddWithValue("@passwordHash", Security.HashPassword(newPassword));
                        }

                        cmd.ExecuteNonQuery();
                    }

                    ReplaceUserClientMappings(conn, tx, user.UserId, user.IsAdmin ? new List<int>() : user.ClientIds);
                    tx.Commit();
                }
            }
        }

        public static void DisableUser(int userId, bool disabled)
        {
            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand("UPDATE dbo.Users SET IsDisabled=@d WHERE UserId=@id", conn))
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@id", userId);
                cmd.Parameters.AddWithValue("@d", disabled);
                cmd.ExecuteNonQuery();
            }
        }

        public static void DeleteUser(int userId)
        {
            using (var conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                using (var tx = conn.BeginTransaction())
                {
                using (var delMappings = new SqlCommand("DELETE FROM dbo.UserClients WHERE UserId=@id", conn, tx))
                {
                    delMappings.Parameters.AddWithValue("@id", userId);
                    delMappings.ExecuteNonQuery();
                }

                using (var delUser = new SqlCommand("DELETE FROM dbo.Users WHERE UserId=@id", conn, tx))
                {
                    delUser.Parameters.AddWithValue("@id", userId);
                    delUser.ExecuteNonQuery();
                }

                    tx.Commit();
                }
            }
        }

        public static string CreatePasswordResetToken(string email)
        {
            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(@"
UPDATE dbo.Users
SET PasswordResetToken = @token,
    PasswordResetExpiresUtc = @expires
WHERE Email = @email
  AND IsDisabled = 0;
SELECT @@ROWCOUNT;", conn))
            {
                conn.Open();
                var token = Guid.NewGuid().ToString("N");
                cmd.Parameters.AddWithValue("@token", token);
                cmd.Parameters.AddWithValue("@expires", DateTime.UtcNow.AddHours(1));
                cmd.Parameters.AddWithValue("@email", email ?? string.Empty);
                var affected = Convert.ToInt32(cmd.ExecuteScalar());
                return affected > 0 ? token : null;
            }
        }

        public static bool ResetPasswordWithToken(string token, string newPassword)
        {
            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(@"
UPDATE dbo.Users
SET PasswordHash = @hash,
    PasswordResetToken = NULL,
    PasswordResetExpiresUtc = NULL
WHERE PasswordResetToken = @token
  AND PasswordResetExpiresUtc IS NOT NULL
  AND PasswordResetExpiresUtc >= @now", conn))
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@hash", Security.HashPassword(newPassword ?? string.Empty));
                cmd.Parameters.AddWithValue("@token", token ?? string.Empty);
                cmd.Parameters.AddWithValue("@now", DateTime.UtcNow);
                return cmd.ExecuteNonQuery() == 1;
            }
        }

        private static void ReplaceUserClientMappings(SqlConnection conn, SqlTransaction tx, int userId, List<int> clientIds)
        {
            using (var delete = new SqlCommand("DELETE FROM dbo.UserClients WHERE UserId=@userId", conn, tx))
            {
                delete.Parameters.AddWithValue("@userId", userId);
                delete.ExecuteNonQuery();
            }

            if (clientIds == null) return;

            foreach (var clientId in clientIds)
            {
                using (var insert = new SqlCommand("INSERT INTO dbo.UserClients(UserId, ClientId) VALUES (@userId, @clientId)", conn, tx))
                {
                    insert.Parameters.AddWithValue("@userId", userId);
                    insert.Parameters.AddWithValue("@clientId", clientId);
                    insert.ExecuteNonQuery();
                }
            }
        }
    }
}
