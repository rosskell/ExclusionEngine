using System.Collections.Generic;

namespace ExclusionEngine.Web
{
    public class UserModel
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsDisabled { get; set; }
    }

    public class UserAdminModel
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsDisabled { get; set; }
        public List<int> ClientIds { get; set; } = new List<int>();
    }

    public class ClientModel
    {
        public int ClientId { get; set; }
        public string ClientCode { get; set; }
        public string ClientName { get; set; }
        public string ClientDisplay => ClientCode + " - " + ClientName;
    }

    public class CustomerEntryInput
    {
        public int EntryId { get; set; }
        public int ClientId { get; set; }
        public string CustomerNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Zip4 { get; set; }
        public string DeliveryPointBarcode { get; set; }
        public string Dpv { get; set; }
        public string Email { get; set; }

        public string FullName => (FirstName + " " + LastName).Trim();
        public string FormattedAddress =>
            string.IsNullOrWhiteSpace(Address2)
                ? $"{Address1}, {City}, {State} {Zip}{(string.IsNullOrWhiteSpace(Zip4) ? string.Empty : "-" + Zip4)}"
                : $"{Address1}, {Address2}, {City}, {State} {Zip}{(string.IsNullOrWhiteSpace(Zip4) ? string.Empty : "-" + Zip4)}";
    }

    public class CassResult
    {
        public bool HasChanges { get; set; }
        public CustomerEntryInput Standardized { get; set; }
    }
}
