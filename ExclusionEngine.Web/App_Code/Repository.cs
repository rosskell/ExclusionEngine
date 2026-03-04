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
    PasswordHash NVARCHAR(200) NOT NULL
);
IF OBJECT_ID('dbo.Clients','U') IS NULL
CREATE TABLE dbo.Clients (
    ClientId INT IDENTITY(1,1) PRIMARY KEY,
    ClientCode NVARCHAR(50) NOT NULL UNIQUE,
    ClientName NVARCHAR(200) NOT NULL
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
    Email NVARCHAR(200) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_CustomerEntries_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId),
    CONSTRAINT FK_CustomerEntries_Clients FOREIGN KEY (ClientId) REFERENCES dbo.Clients(ClientId)
);";
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
    INSERT INTO dbo.Users (Username, PasswordHash) VALUES ('demo', @demoPasswordHash);
END
IF NOT EXISTS (SELECT 1 FROM dbo.Clients WHERE ClientCode = 'CLI-ALPHA')
BEGIN
    INSERT INTO dbo.Clients (ClientCode, ClientName) VALUES ('CLI-ALPHA', 'Alpha Logistics');
END
IF NOT EXISTS (SELECT 1 FROM dbo.Clients WHERE ClientCode = 'CLI-BETA')
BEGIN
    INSERT INTO dbo.Clients (ClientCode, ClientName) VALUES ('CLI-BETA', 'Beta Retail');
END
IF EXISTS (SELECT 1 FROM dbo.Users WHERE Username = 'demo')
BEGIN
    DECLARE @UserId INT = (SELECT UserId FROM dbo.Users WHERE Username = 'demo');
    INSERT INTO dbo.UserClients (UserId, ClientId)
    SELECT @UserId, c.ClientId
    FROM dbo.Clients c
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.UserClients uc WHERE uc.UserId = @UserId AND uc.ClientId = c.ClientId
    );
END", conn);
                cmd.Parameters.AddWithValue("@demoPasswordHash", Security.HashPassword("demo123"));
                cmd.ExecuteNonQuery();
            }
        }

        public static UserModel ValidateUser(string username, string password)
        {
            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand("SELECT UserId, Username, PasswordHash FROM dbo.Users WHERE Username = @u", conn))
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@u", username ?? string.Empty);
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read()) return null;
                    var hash = reader.GetString(2);
                    if (!Security.VerifyPassword(password, hash)) return null;

                    return new UserModel
                    {
                        UserId = reader.GetInt32(0),
                        Username = reader.GetString(1)
                    };
                }
            }
        }

        public static List<ClientModel> GetClientsForUser(int userId)
        {
            var result = new List<ClientModel>();
            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(@"
SELECT c.ClientId, c.ClientCode, c.ClientName
FROM dbo.Clients c
INNER JOIN dbo.UserClients uc ON c.ClientId = uc.ClientId
WHERE uc.UserId = @u
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
                            ClientName = reader.GetString(2)
                        });
                    }
                }
            }

            return result;
        }

        public static bool UserHasClientAccess(int userId, int clientId)
        {
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
(UserId, ClientId, CustomerNumber, FirstName, LastName, Address1, Address2, City, State, Zip, Email)
VALUES
(@u, @c, @n, @fn, @ln, @a1, @a2, @city, @st, @zip, @email)", conn))
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
                cmd.Parameters.AddWithValue("@email", string.IsNullOrWhiteSpace(input.Email) ? (object)DBNull.Value : input.Email);
                cmd.ExecuteNonQuery();
            }
        }

        public static CustomerEntryInput GetEntryForEdit(int userId, int entryId)
        {
            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(@"
SELECT ce.EntryId, ce.ClientId, ce.CustomerNumber, ce.FirstName, ce.LastName,
       ce.Address1, ce.Address2, ce.City, ce.State, ce.Zip, ce.Email
FROM dbo.CustomerEntries ce
INNER JOIN dbo.UserClients uc ON uc.ClientId = ce.ClientId AND uc.UserId = @u
WHERE ce.EntryId = @entryId", conn))
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@u", userId);
                cmd.Parameters.AddWithValue("@entryId", entryId);

                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read()) return null;

                    return new CustomerEntryInput
                    {
                        EntryId = reader.GetInt32(0),
                        ClientId = reader.GetInt32(1),
                        CustomerNumber = reader.GetString(2),
                        FirstName = reader.GetString(3),
                        LastName = reader.GetString(4),
                        Address1 = reader.GetString(5),
                        Address2 = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                        City = reader.GetString(7),
                        State = reader.GetString(8),
                        Zip = reader.GetString(9),
                        Email = reader.IsDBNull(10) ? string.Empty : reader.GetString(10)
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
UPDATE ce
SET ce.ClientId = @c,
    ce.CustomerNumber = @n,
    ce.FirstName = @fn,
    ce.LastName = @ln,
    ce.Address1 = @a1,
    ce.Address2 = @a2,
    ce.City = @city,
    ce.State = @st,
    ce.Zip = @zip,
    ce.Email = @email
FROM dbo.CustomerEntries ce
INNER JOIN dbo.UserClients uc ON uc.ClientId = ce.ClientId AND uc.UserId = @u
WHERE ce.EntryId = @entryId", conn))
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@u", userId);
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
                cmd.Parameters.AddWithValue("@email", string.IsNullOrWhiteSpace(input.Email) ? (object)DBNull.Value : input.Email);

                var affected = cmd.ExecuteNonQuery();
                if (affected == 0)
                {
                    throw new InvalidOperationException("Entry not found or not authorized.");
                }
            }
        }

        public static DataTable GetRecentEntriesForUser(int userId)
        {
            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(@"
SELECT TOP 20
    ce.EntryId,
    c.ClientCode + ' - ' + c.ClientName AS ClientName,
    ce.CustomerNumber,
    ce.FirstName + ' ' + ce.LastName AS FullName,
    ce.Address1 + COALESCE(', ' + NULLIF(ce.Address2,''), '') + ', ' + ce.City + ', ' + ce.State + ' ' + ce.Zip AS FormattedAddress,
    ce.Email,
    ce.CreatedAt
FROM dbo.CustomerEntries ce
INNER JOIN dbo.Clients c ON ce.ClientId = c.ClientId
INNER JOIN dbo.UserClients uc ON uc.ClientId = ce.ClientId
WHERE uc.UserId = @u
ORDER BY ce.CreatedAt DESC", conn))
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@u", userId);
                var dt = new DataTable();
                dt.Load(cmd.ExecuteReader());
                return dt;
            }
        }
    }
}
