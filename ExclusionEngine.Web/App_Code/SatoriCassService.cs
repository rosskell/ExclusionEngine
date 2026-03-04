using System;
using System.Configuration;

namespace ExclusionEngine.Web
{
    public static class SatoriCassService
    {
        public static CassResult StandardizeAddress(CustomerEntryInput input)
        {
            // Placeholder for BCC Satori CASS server integration.
            // Use these app settings when wiring your real API client.
            var endpoint = ConfigurationManager.AppSettings["SatoriCassEndpoint"];
            var username = ConfigurationManager.AppSettings["SatoriCassUsername"];
            var password = ConfigurationManager.AppSettings["SatoriCassPassword"];
            _ = endpoint;
            _ = username;
            _ = password;

            var standardized = new CustomerEntryInput
            {
                ClientId = input.ClientId,
                CustomerNumber = input.CustomerNumber,
                FirstName = input.FirstName,
                LastName = input.LastName,
                Address1 = ToTitleCase(input.Address1),
                Address2 = ToTitleCase(input.Address2),
                City = ToTitleCase(input.City),
                State = (input.State ?? string.Empty).Trim().ToUpperInvariant(),
                Zip = NormalizeZip(input.Zip),
                Email = input.Email
            };

            return new CassResult
            {
                Standardized = standardized,
                HasChanges = !string.Equals(input.FormattedAddress, standardized.FormattedAddress, StringComparison.Ordinal)
            };
        }

        private static string ToTitleCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.Trim().ToLowerInvariant());
        }

        private static string NormalizeZip(string zip)
        {
            if (string.IsNullOrWhiteSpace(zip)) return string.Empty;
            return zip.Trim();
        }
    }
}
