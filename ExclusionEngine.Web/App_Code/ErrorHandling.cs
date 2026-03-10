using System;
using System.Diagnostics;
using System.Web;

namespace ExclusionEngine.Web
{
    public static class ErrorHandling
    {
        public static string ToUserMessage(Exception ex)
        {
            var reference = LogException(ex);
            return $"<span class='error'>Something went wrong. Please try again. Reference: {reference}</span>";
        }

        public static string LogException(Exception ex)
        {
            var reference = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpperInvariant();
            var ctx = HttpContext.Current;
            var path = ctx?.Request?.RawUrl ?? "(unknown)";
            var user = "anonymous";

            try
            {
                if (ctx?.Session?["Username"] is string username && !string.IsNullOrWhiteSpace(username))
                {
                    user = username;
                }
            }
            catch
            {
                // Session can be unavailable depending on request pipeline stage.
            }

            Trace.TraceError($"[{reference}] Exception. User={user}; Path={path}; Details={ex}");
            return reference;
        }
    }
}
