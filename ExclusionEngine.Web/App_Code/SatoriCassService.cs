using System;
using System.Configuration;
using System.Globalization;
using System.Reflection;

namespace ExclusionEngine.Web
{
    public static class SatoriCassService
    {
        public static CassResult StandardizeAddress(CustomerEntryInput input)
        {
            var fallback = BuildFallbackStandardized(input);
            object task = null;

            try
            {
                task = CreateZipTask();
                if (task == null)
                {
                    return BuildResult(input, fallback);
                }

                var server = (ConfigurationManager.AppSettings["SatoriCassMailRoomServer"]
                              ?? ConfigurationManager.AppSettings["SatoriCassEndpoint"]
                              ?? string.Empty).Trim();

                if (!string.IsNullOrWhiteSpace(server))
                {
                    InvokeSetMailRoomServer(task, server);
                }

                InvokeNoArg(task, "PrepareTask");
                InvokeNoArg(task, "ClearAddress");

                SetProperty(task, "Capitalize", 1);
                SetProperty(task, "BusinessName", BuildBusinessName(input));
                SetProperty(task, "AddressLine1", input.Address1 ?? string.Empty);
                SetProperty(task, "City", input.City ?? string.Empty);
                SetProperty(task, "State", (input.State ?? string.Empty).Trim());
                SetProperty(task, "ZipCode", input.Zip ?? string.Empty);
                SetProperty(task, "CarrierRoute", string.Empty);

                InvokeNoArg(task, "CheckAddress");

                var errorCodes = Convert.ToInt32(GetProperty(task, "ErrorCodes"));
                if (errorCodes >= 100 && errorCodes < 500)
                {
                    return BuildResult(input, fallback);
                }

                var standardized = new CustomerEntryInput
                {
                    ClientId = input.ClientId,
                    CustomerNumber = input.CustomerNumber,
                    FirstName = input.FirstName,
                    LastName = input.LastName,
                    Address1 = (GetProperty(task, "AddressLine1") ?? string.Empty).ToString().Trim(),
                    Address2 = ToTitleCase(input.Address2),
                    City = (GetProperty(task, "City") ?? string.Empty).ToString().Trim(),
                    State = (GetProperty(task, "State") ?? string.Empty).ToString().Trim().ToUpperInvariant(),
                    Zip = NormalizeZip((GetProperty(task, "ZipCode") ?? string.Empty).ToString()),
                    Email = input.Email
                };

                return BuildResult(input, standardized);
            }
            catch
            {
                return BuildResult(input, fallback);
            }
            finally
            {
                if (task != null)
                {
                    try
                    {
                        InvokeNoArg(task, "EndTask");
                    }
                    catch
                    {
                        // Ignore cleanup failures.
                    }
                }
            }
        }

        private static object CreateZipTask()
        {
            var progIds = new[]
            {
                "MRTKTASKLib.ZIPTaskClass",
                "MRTKTASKLib.ZIPTask",
                "MailroomToolkitTasks.ZIPTask"
            };

            foreach (var progId in progIds)
            {
                var type = Type.GetTypeFromProgID(progId, false);
                if (type != null)
                {
                    return Activator.CreateInstance(type);
                }
            }

            return null;
        }

        private static void InvokeSetMailRoomServer(object task, string server)
        {
            var args = new object[] { server };
            var modifiers = new ParameterModifier(1);
            modifiers[0] = true;

            task.GetType().InvokeMember(
                "set_MailRoomServer",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod,
                null,
                task,
                args,
                new[] { modifiers },
                null,
                null);
        }

        private static void InvokeNoArg(object instance, string methodName)
        {
            instance.GetType().InvokeMember(
                methodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod,
                null,
                instance,
                new object[0]);
        }

        private static void SetProperty(object instance, string propertyName, object value)
        {
            instance.GetType().InvokeMember(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty,
                null,
                instance,
                new[] { value });
        }

        private static object GetProperty(object instance, string propertyName)
        {
            return instance.GetType().InvokeMember(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty,
                null,
                instance,
                new object[0]);
        }

        private static string BuildBusinessName(CustomerEntryInput input)
        {
            return ((input.FirstName ?? string.Empty) + " " + (input.LastName ?? string.Empty)).Trim();
        }

        private static CassResult BuildResult(CustomerEntryInput input, CustomerEntryInput standardized)
        {
            return new CassResult
            {
                Standardized = standardized,
                HasChanges = !string.Equals(input.FormattedAddress, standardized.FormattedAddress, StringComparison.Ordinal)
            };
        }

        private static CustomerEntryInput BuildFallbackStandardized(CustomerEntryInput input)
        {
            return new CustomerEntryInput
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
        }

        private static string ToTitleCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.Trim().ToLowerInvariant());
        }

        private static string NormalizeZip(string zip)
        {
            if (string.IsNullOrWhiteSpace(zip)) return string.Empty;
            return zip.Trim();
        }
    }
}
