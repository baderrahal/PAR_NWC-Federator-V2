using System;
using System.Reflection;

namespace Federator.Core.Diagnostics
{
    /// <summary>
    /// Which binary this actually is. The plain assembly version is 1.0.0.0 and always
    /// will be, so a stale install cannot be spotted from a log. The build stamps the git
    /// commit and the moment it was built into AssemblyInformationalVersion, which is a
    /// free text field, and this reads it back. See Directory.Build.targets.
    /// </summary>
    public static class BuildStamp
    {
        public const string Unknown = "UNKNOWN, the assembly carries no build stamp";

        /// <summary>The whole stamp as the build wrote it, or a reason when there is none.</summary>
        public static string Of(Assembly assembly)
        {
            if (assembly == null)
            {
                return Unknown;
            }

            try
            {
                object[] found = assembly.GetCustomAttributes(
                    typeof(AssemblyInformationalVersionAttribute), false);

                if (found.Length == 0)
                {
                    return Unknown;
                }

                string stamp = ((AssemblyInformationalVersionAttribute)found[0]).InformationalVersion;
                return string.IsNullOrEmpty(stamp) ? Unknown : stamp;
            }
            catch (Exception error)
            {
                return "UNKNOWN, reading the build stamp threw "
                    + error.GetType().Name + ": " + error.Message;
            }
        }

        /// <summary>The stamp of the assembly this type lives in.</summary>
        public static string OfCore()
        {
            return Of(typeof(BuildStamp).Assembly);
        }

        /// <summary>
        /// True when the stamp looks like one this build produced rather than a bare
        /// version number. Used by the tests to prove the stamping actually ran.
        /// </summary>
        public static bool LooksStamped(string stamp)
        {
            return !string.IsNullOrEmpty(stamp)
                && stamp.IndexOf(" built ", StringComparison.Ordinal) > 0;
        }
    }
}
