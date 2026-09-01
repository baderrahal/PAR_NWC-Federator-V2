using System;
using System.Globalization;
using System.Reflection;
using System.Text;
using Autodesk.Navisworks.Api;
using Autodesk.Navisworks.Api.ApplicationParts;
using NavisworksApplication = Autodesk.Navisworks.Api.Application;

namespace Federator.Addin.Engine
{
    /// <summary>
    /// The few facts the log header needs out of the running application. Every one of
    /// these is read defensively, because a header that throws would cost the whole log.
    /// </summary>
    public static class NavisworksFacts
    {
        /// <summary>
        /// ApplicationVersion does not override ToString, so the string is built from its
        /// properties. See docs\scan.md section 4c.
        /// </summary>
        public static string VersionString()
        {
            try
            {
                ApplicationVersion version = NavisworksApplication.Version;

                if (version == null)
                {
                    return "UNKNOWN, Application.Version was null";
                }

                StringBuilder text = new StringBuilder();
                text.Append(Or(version.RuntimeProductName, "unnamed product"));
                text.Append("  runtime ").Append(Number(version.RuntimeMajor))
                    .Append('.').Append(Number(version.RuntimeMinor));
                text.Append("  build ").Append(Number(version.Build));
                text.Append("  api ").Append(Number(version.ApiMajor))
                    .Append('.').Append(Number(version.ApiMinor));
                text.Append(version.IsApiStable ? " (stable)" : " (not stable)");

                if (version.IsRuntimeBeta)
                {
                    text.Append("  RUNTIME IS BETA");
                }

                text.Append("  language ").Append(Or(version.RuntimeLanguage, "unknown"));
                text.Append("  runtime string ").Append(Or(version.Runtime, "none"));
                return text.ToString();
            }
            catch (Exception error)
            {
                return "UNKNOWN, reading Application.Version threw "
                    + error.GetType().FullName + ": " + error.Message;
            }
        }

        /// <summary>
        /// The folder Navisworks is installed in, read as the folder its own API assembly
        /// was loaded from. That is the install, whatever drive it is on, so nothing here
        /// is a hard coded path and no search is needed.
        /// </summary>
        public static string InstallFolder()
        {
            try
            {
                string dll = typeof(NavisworksApplication).Assembly.Location;

                return string.IsNullOrEmpty(dll)
                    ? string.Empty
                    : System.IO.Path.GetDirectoryName(dll) ?? string.Empty;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// The language folder name the application reports, for example en-US. Empty
        /// where it cannot be read, and the caller then falls back to en-US.
        /// </summary>
        public static string Language()
        {
            try
            {
                ApplicationVersion version = NavisworksApplication.Version;

                return version == null ? string.Empty : Or(version.RuntimeLanguage, string.Empty);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        /// <summary>The document open right now, with its path, or the word none.</summary>
        public static string OpenDocument()
        {
            try
            {
                Document document = NavisworksApplication.ActiveDocument;

                if (document == null)
                {
                    return "none, there is no active document";
                }

                if (document.IsClear)
                {
                    return "none, the document is clear";
                }

                string name = Or(document.CurrentFileName, document.FileName);
                int models = document.Models == null ? 0 : document.Models.Count;

                return Or(name, "an unsaved document")
                    + "  (" + models + (models == 1 ? " model" : " models") + " loaded)";
            }
            catch (Exception error)
            {
                return "UNKNOWN, reading the active document threw "
                    + error.GetType().FullName + ": " + error.Message;
            }
        }

        /// <summary>
        /// The build stamp, not the bare assembly version. The version is 1.0.0.0 and
        /// always will be, so on its own it cannot tell a fresh install from a stale one.
        /// The stamp carries the commit and the moment the binary was built.
        /// </summary>
        public static string PluginVersion()
        {
            try
            {
                return Federator.Core.Diagnostics.BuildStamp.Of(typeof(FederatorPlugin).Assembly);
            }
            catch (Exception error)
            {
                return "UNKNOWN, " + error.GetType().Name + ": " + error.Message;
            }
        }

        private static string Number(int value)
        {
            return value.ToString(CultureInfo.InvariantCulture);
        }

        private static string Or(string value, string fallback)
        {
            return string.IsNullOrEmpty(value) ? fallback : value;
        }
    }
}
