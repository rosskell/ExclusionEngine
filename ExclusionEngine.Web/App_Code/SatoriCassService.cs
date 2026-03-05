using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace ExclusionEngine.Web
{
    public static class SatoriCassService
    {
        public static CassResult StandardizeAddress(CustomerEntryInput input)
        {
            var fallback = BuildFallbackStandardized(input);
            object task = null;
            var traceEnabled = IsTruthy(ConfigurationManager.AppSettings["SatoriCassTrace"]);
            var throwOnError = IsTruthy(ConfigurationManager.AppSettings["SatoriCassThrowOnError"]);
            var requireServerAssignment = IsTruthy(ConfigurationManager.AppSettings["SatoriCassRequireServerAssignment"]);

            try
            {
                TraceLog(traceEnabled, "Starting CASS standardization.");
                task = CreateZipTask();
                if (task == null)
                {
                    TraceLog(traceEnabled,
                        "Unable to create ZIP task COM object. Ensure Mailroom Toolkit COM is installed/registered and bitness matches IIS worker process. " +
                        "Do not add Interop.MRTKTASKLib.dll as a web project assembly reference.");
                    if (throwOnError)
                    {
                        throw new InvalidOperationException("CASS COM object could not be created. See logs for details.");
                    }

                    return BuildResult(input, fallback, true, "Unable to initialize CASS ZIP task.");
                }

                var server = (ConfigurationManager.AppSettings["SatoriCassMailRoomServer"]
                              ?? ConfigurationManager.AppSettings["SatoriCassEndpoint"]
                              ?? string.Empty).Trim();

                if (!string.IsNullOrWhiteSpace(server))
                {
                    TraceLog(traceEnabled, "Configuring MailRoom server: " + server);
                    var assigned = ConfigureMailRoomServer(task, server, traceEnabled);
                    if (!assigned)
                    {
                        var msg = "Failed to assign MailRoom server on ZIP task object. " +
                                  "Tried method/property variants for MailRoomServer.";
                        TraceLog(traceEnabled, msg);
                        if (requireServerAssignment || throwOnError)
                        {
                            throw new InvalidOperationException(msg);
                        }
                    }
                }
                else
                {
                    TraceLog(traceEnabled, "No SatoriCassMailRoomServer/SatoriCassEndpoint configured; using COM component default server behavior.");
                }

                TraceLog(traceEnabled, "Preparing ZIP task.");
                InvokeNoArg(task, "PrepareTask");
                InvokeNoArg(task, "ClearAddress");

                SetProperty(task, "Capitalize", 1);
                SetProperty(task, "BusinessName", BuildBusinessName(input));
                SetProperty(task, "AddressLine1", input.Address1 ?? string.Empty);
                SetProperty(task, "City", input.City ?? string.Empty);
                SetProperty(task, "State", (input.State ?? string.Empty).Trim());
                SetProperty(task, "ZipCode", CombineZip(input.Zip, input.Zip4));
                SetProperty(task, "CarrierRoute", string.Empty);

                TraceLog(traceEnabled, "Sending address to CASS via CheckAddress().");
                InvokeNoArg(task, "CheckAddress");

                var errorCodes = Convert.ToInt32(GetProperty(task, "ErrorCodes"));
                TraceLog(traceEnabled, "CASS ErrorCodes returned: " + errorCodes);
                if (errorCodes >= 100 && errorCodes < 500)
                {
                    TraceLog(traceEnabled, "CASS returned error range 100-499; using fallback normalization.");
                    if (throwOnError)
                    {
                        throw new InvalidOperationException("CASS returned error code: " + errorCodes);
                    }

                    return BuildResult(input, fallback, true, BuildCassErrorMessage(task, errorCodes));
                }

                var standardizedZipRaw = (GetProperty(task, "ZipCode") ?? string.Empty).ToString();
                ParseZip(standardizedZipRaw, out var zip5, out var zip4);

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
                    Zip = zip5,
                    Zip4 = zip4,
                    DeliveryPointBarcode = ExtractLast3Digits(GetOptionalProperty(task, "DPBarcodeString")),
                    Dpv = FirstNonEmpty(
                        GetOptionalProperty(task, "DPV"),
                        GetOptionalProperty(task, "DPVCode"),
                        GetOptionalProperty(task, "DPVStatus"),
                        GetOptionalProperty(task, "DPVConfirmation")),
                    Email = input.Email
                };

                return BuildResult(input, standardized, false, string.Empty);
            }
            catch (Exception ex)
            {
                TraceLog(traceEnabled, "CASS call failed: " + ex);
                if (throwOnError)
                {
                    throw;
                }

                return BuildResult(input, fallback, true, ex.Message);
            }
            finally
            {
                if (task != null)
                {
                    try
                    {
                        TraceLog(traceEnabled, "Ending ZIP task.");
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
            var traceEnabled = IsTruthy(ConfigurationManager.AppSettings["SatoriCassTrace"]);
            var configuredProgId = (ConfigurationManager.AppSettings["SatoriCassProgId"] ?? string.Empty).Trim();
            var configuredClsid = (ConfigurationManager.AppSettings["SatoriCassClsid"] ?? string.Empty).Trim();
            var interopPath = (ConfigurationManager.AppSettings["SatoriCassInteropPath"] ?? string.Empty).Trim();
            var interopTypeName = (ConfigurationManager.AppSettings["SatoriCassInteropType"] ?? "MRTKTASKLib.ZIPTaskClass").Trim();
            var enableInteropPathLoad = IsTruthy(ConfigurationManager.AppSettings["SatoriCassEnableInteropPathLoad"]);

            var progIds = new[]
            {
                configuredProgId,
                "MRTKTASKLib.ZIPTask",
                "MRTKTASKLib.ZIPTaskClass",
                "MailroomToolkitTasks.ZIPTask"
            };

            foreach (var progId in progIds)
            {
                if (string.IsNullOrWhiteSpace(progId)) continue;
                try
                {
                    TraceLog(traceEnabled, "Trying CASS activation via ProgID: " + progId);
                    var type = Type.GetTypeFromProgID(progId, false);
                    if (type != null)
                    {
                        return Activator.CreateInstance(type);
                    }
                }
                catch (Exception ex)
                {
                    TraceLog(traceEnabled, "ProgID activation failed for " + progId + ": " + ex.Message);
                }
            }

            if (!string.IsNullOrWhiteSpace(configuredClsid) && Guid.TryParse(configuredClsid, out var clsid))
            {
                try
                {
                    TraceLog(traceEnabled, "Trying CASS activation via CLSID: " + configuredClsid);
                    var clsidType = Type.GetTypeFromCLSID(clsid, false);
                    if (clsidType != null)
                    {
                        return Activator.CreateInstance(clsidType);
                    }
                }
                catch (Exception ex)
                {
                    TraceLog(traceEnabled, "CLSID activation failed for " + configuredClsid + ": " + ex.Message);
                }
            }

            if (enableInteropPathLoad && !string.IsNullOrWhiteSpace(interopPath) && File.Exists(interopPath))
            {
                try
                {
                    TraceLog(traceEnabled, "Trying CASS activation via interop assembly path: " + interopPath);
                    var asm = Assembly.LoadFrom(interopPath);
                    var interopType = asm.GetType(interopTypeName, false);
                    if (interopType != null)
                    {
                        return Activator.CreateInstance(interopType);
                    }

                    TraceLog(traceEnabled, "Interop type not found: " + interopTypeName);
                }
                catch (Exception ex)
                {
                    TraceLog(traceEnabled, "Interop assembly activation failed: " + ex.Message);
                }
            }

            if (!enableInteropPathLoad && !string.IsNullOrWhiteSpace(interopPath))
            {
                TraceLog(traceEnabled, "Skipping interop assembly path load because SatoriCassEnableInteropPathLoad is false.");
            }

            TraceLog(traceEnabled, "CreateZipTask failed for all configured activation routes.");
            return null;
        }

        private static bool ConfigureMailRoomServer(object task, string server, bool traceEnabled)
        {
            var type = task.GetType();

            // 1) COM-style by-ref setter method: set_MailRoomServer(ref string)
            try
            {
                var method = type.GetMethod("set_MailRoomServer", BindingFlags.Instance | BindingFlags.Public);
                if (method != null)
                {
                    var args = new object[] { server };
                    method.Invoke(task, args);
                    return true;
                }
            }
            catch (Exception ex)
            {
                TraceLog(traceEnabled, "set_MailRoomServer(ref string) failed: " + ex.Message);
            }

            // 2) Direct property setter: MailRoomServer = "host:port"
            try
            {
                var prop = type.GetProperty("MailRoomServer", BindingFlags.Instance | BindingFlags.Public);
                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(task, server, null);
                    return true;
                }
            }
            catch (Exception ex)
            {
                TraceLog(traceEnabled, "MailRoomServer property setter failed: " + ex.Message);
            }

            // 3) Alternative method names seen in some interop wrappers
            var altNames = new[] { "SetMailRoomServer", "Set_MailRoomServer" };
            foreach (var name in altNames)
            {
                try
                {
                    var m = type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public);
                    if (m != null)
                    {
                        m.Invoke(task, new object[] { server });
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    TraceLog(traceEnabled, name + " failed: " + ex.Message);
                }
            }

            return false;
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

        private static CassResult BuildResult(CustomerEntryInput input, CustomerEntryInput standardized, bool hasError, string errorMessage)
        {
            return new CassResult
            {
                Standardized = standardized,
                HasChanges = !string.Equals(input.FormattedAddress, standardized.FormattedAddress, StringComparison.Ordinal),
                HasError = hasError,
                ErrorMessage = errorMessage ?? string.Empty
            };
        }

        private static string BuildCassErrorMessage(object task, int errorCode)
        {
            var message = string.Empty;
            try
            {
                var result = task.GetType().InvokeMember(
                    "get_ErrorCodeString",
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod,
                    null,
                    task,
                    new object[] { 1 });
                message = result == null ? string.Empty : result.ToString();
            }
            catch
            {
                // best effort only
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                return "CASS validation failed with code " + errorCode + ".";
            }

            return "CASS validation failed with code " + errorCode + ": " + message;
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
                Zip4 = NormalizeZip4(input.Zip4),
                DeliveryPointBarcode = ExtractLast3Digits(input.DeliveryPointBarcode),
                Dpv = input.Dpv,
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
            ParseZip(zip, out var zip5, out _);
            return zip5;
        }

        private static string NormalizeZip4(string zip4)
        {
            if (string.IsNullOrWhiteSpace(zip4)) return string.Empty;
            var digits = DigitsOnly(zip4);
            return digits.Length >= 4 ? digits.Substring(0, 4) : digits;
        }

        private static void ParseZip(string zip, out string zip5, out string zip4)
        {
            var digits = DigitsOnly(zip);
            zip5 = digits.Length >= 5 ? digits.Substring(0, 5) : digits;
            zip4 = digits.Length >= 9 ? digits.Substring(5, 4) : string.Empty;
        }

        private static string DigitsOnly(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            var chars = value.Trim().ToCharArray();
            var buffer = new System.Text.StringBuilder(chars.Length);
            foreach (var c in chars)
            {
                if (char.IsDigit(c)) buffer.Append(c);
            }

            return buffer.ToString();
        }

        private static string CombineZip(string zip5, string zip4)
        {
            var z5 = NormalizeZip(zip5);
            var z4 = NormalizeZip4(zip4);
            if (string.IsNullOrWhiteSpace(z4)) return z5;
            return z5 + "-" + z4;
        }

        private static string GetOptionalProperty(object instance, string propertyName)
        {
            try
            {
                var value = GetProperty(instance, propertyName);
                return value == null ? string.Empty : value.ToString().Trim();
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string FirstNonEmpty(params string[] values)
        {
            if (values == null) return string.Empty;
            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value)) return value;
            }

            return string.Empty;
        }

        private static string ExtractLast3Digits(string value)
        {
            var digits = DigitsOnly(value);
            if (digits.Length <= 3) return digits;
            return digits.Substring(digits.Length - 3, 3);
        }

        private static bool IsTruthy(string value)
        {
            return string.Equals((value ?? string.Empty).Trim(), "1", StringComparison.OrdinalIgnoreCase)
                || string.Equals((value ?? string.Empty).Trim(), "true", StringComparison.OrdinalIgnoreCase)
                || string.Equals((value ?? string.Empty).Trim(), "yes", StringComparison.OrdinalIgnoreCase);
        }

        private static void TraceLog(bool enabled, string message)
        {
            if (!enabled) return;
            Trace.WriteLine("[SatoriCassService] " + message);
        }
    }
}
