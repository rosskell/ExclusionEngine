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
    CONSTRAINT PK_UserClients PRIMARY KEY (UserId, ClientId)
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
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
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
    INSERT INTO dbo.Users (Username, PasswordHash) VALUES ('demo', 'demo123');
    INSERT INTO dbo.Clients (ClientCode, ClientName) VALUES ('CLI-ALPHA', 'Alpha Logistics');
    INSERT INTO dbo.Clients (ClientCode, ClientName) VALUES ('CLI-BETA', 'Beta Retail');
    DECLARE @UserId INT = (SELECT UserId FROM dbo.Users WHERE Username = 'demo');
    INSERT INTO dbo.UserClients (UserId, ClientId)
    SELECT @UserId, ClientId FROM dbo.Clients;
END", conn);
                cmd.ExecuteNonQuery();
            }
        }

        public static UserModel ValidateUser(string username, string password)
        {
            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand("SELECT UserId, Username FROM dbo.Users WHERE Username=@u AND PasswordHash=@p", conn))
            {
                conn.Open();
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@p", password);
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read()) return null;
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

        public static void SaveEntry(int userId, CustomerEntryInput input)
        {
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
                cmd.Parameters.AddWithValue("@a2", (object)input.Address2 ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@city", input.City ?? string.Empty);
                cmd.Parameters.AddWithValue("@st", input.State ?? string.Empty);
                cmd.Parameters.AddWithValue("@zip", input.Zip ?? string.Empty);
                cmd.Parameters.AddWithValue("@email", (object)input.Email ?? DBNull.Value);
                cmd.ExecuteNonQuery();
            }
        }

        public static DataTable GetRecentEntriesForUser(int userId)
        {
            using (var conn = new SqlConnection(ConnectionString))
            using (var cmd = new SqlCommand(@"
SELECT TOP 20
    c.ClientCode + ' - ' + c.ClientName AS ClientName,
    ce.CustomerNumber,
    ce.FirstName + ' ' + ce.LastName AS FullName,
    ce.Address1 + COALESCE(', ' + NULLIF(ce.Address2,''), '') + ', ' + ce.City + ', ' + ce.State + ' ' + ce.Zip AS FormattedAddress,
    ce.Email,
    ce.CreatedAt
FROM dbo.CustomerEntries ce
INNER JOIN dbo.Clients c ON ce.ClientId = c.ClientId
WHERE ce.UserId = @u
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
